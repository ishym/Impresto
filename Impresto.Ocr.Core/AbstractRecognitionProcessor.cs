using Gradient.ObjectSerializer;
using Gradient.RecognitionTemplate;
using IDCaptureWCFSupport;
using IDCaptureWCFSupport.Processor;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

namespace Impresto.Ocr.Core
{
    public abstract class AbstractRecognitionProcessor
    {
        public abstract string SdkPath { get; }
        public abstract string DetectKey { get; }
        public abstract string DetectRegex { get; }

        private string _enginePath;
        private string _templatePath;
        private string _filterPath;
        private string _documentPath;
        private string _dictionaryPath;

        #region Virtual Methods

        protected virtual void OnInit(TimeSpan timestamp)
        {
        }

        protected virtual void OnTry(string template)
        {
        }

        protected virtual void OnSetFilter(string filter)
        {
        }

        protected virtual void OnRecognizeData(TimeSpan timestamp, string template)
        {
        }

        protected virtual void OnRecognizeDataByTemplate(TimeSpan timestamp, string template)
        {
        }

        protected virtual void OnError(Exception ex)
        {
        }

        #endregion

        #region Public Methods

        public void Init(string sdkPath, string enginePath = "Engine", string templatePath = "Templates", string filterPath = "Filters", string dictionaryPath = "Dictionaries", string documentPath = "Documents")
        {
            try
            {
                DateTime timestamp = DateTime.Now;

                _enginePath = Path.Combine(sdkPath, enginePath);
                if (!Directory.Exists(_enginePath))
                {
                    Directory.CreateDirectory(_enginePath);
                }

                _templatePath = Path.Combine(sdkPath, templatePath);
                if (!Directory.Exists(_templatePath))
                {
                    Directory.CreateDirectory(_templatePath);
                }

                _filterPath = Path.Combine(sdkPath, filterPath);
                if (!Directory.Exists(_filterPath))
                {
                    Directory.CreateDirectory(_filterPath);
                }

                _dictionaryPath = Path.Combine(sdkPath, dictionaryPath);
                if (!Directory.Exists(_dictionaryPath))
                {
                    Directory.CreateDirectory(_dictionaryPath);
                }

                _documentPath = Path.Combine(sdkPath, documentPath);
                if (!Directory.Exists(_documentPath))
                {
                    Directory.CreateDirectory(_documentPath);
                }

                // initialization of the recognition processor
                FulltextProcessor.InitializeGlobalProcessor(Environment.ProcessorCount, FulltextProcessor.GetEnginePath(_enginePath), "Impresto_{0:00}", string.Empty, _enginePath, string.Empty);

                // configuration of the recognition processor
                FulltextProcessor.CurrentProcessor.SetIDCaptureProperties(_dictionaryPath, true, string.Empty);

                OnInit(DateTime.Now - timestamp);
            }
            catch(Exception ex)
            {
                OnError(ex);
            }
        }
        public virtual Dictionary<string, object> GetRecognizeData(byte[] image, string condition = "")
        {
            Dictionary<string, object> dic = new Dictionary<string, object>();

            try
            {
                DateTime timestamp = DateTime.Now;

                var templates = Directory.EnumerateFiles(_templatePath).Where(f => new[] { ".xml" }.Contains(Path.GetExtension(f)));
                foreach (string template in templates.Where(t => t.Contains(condition)))
                {
                    OnTry(template);

                    dic = GetRecognizeDataByTemplate(image, template);
                    if (dic.Any())
                    {
                        OnRecognizeData(DateTime.Now - timestamp, template);
                        return dic;
                    }
                }
            }
            catch(Exception ex)
            {
                OnError(ex);
            }

            return dic;
        }
        public virtual Dictionary<string, object> GetRecognizeDataByTemplate(byte[] image, string template)
        {
            Dictionary<string, object> dic = new Dictionary<string, object>();

            try
            {
                DateTime timestamp = DateTime.Now;

                OCRObject ocr = new OCRObject()
                {
                    IDCaptureRequest = new IDCaptureWCFSupport.RecognizeDataInputWCF(),
                    OCRObjectType = OCRObjectType.IDCapture, // this parameters tells system to recognize documents using templates
                    BatchName = Path.GetFileName(Path.GetDirectoryName(_documentPath)), // gradient direcotry name
                    BatchesDirectory = Path.GetDirectoryName(Path.GetDirectoryName(_documentPath)), // path of the gradient direcotry
                };

                // loading templates from disk
                List<Gradient.RecognitionTemplate.RecoData> templates = new List<Gradient.RecognitionTemplate.RecoData>()
                {
                    Gradient.ObjectSerializer.ObjectSerializer.Load<Gradient.RecognitionTemplate.RecoData>(template)
                };

                ocr.IDCaptureRequest.Threshold = 0;
                ocr.IDCaptureRequest.ImageData = image; // image bytes
                ocr.IDCaptureRequest.Templates = new IDCaptureWCFSupport.TemplatesWCF();
                ocr.IDCaptureRequest.Templates.Templates = ObjectSerializer.DeepClone<RecoData[]>(templates.ToArray());

                string [] parts = template.Split(new string[] { @"\" },  StringSplitOptions.RemoveEmptyEntries);
                if (parts.Length > 0)
                {
                    string filterStrongName = parts[parts.Length - 1].Replace(".xml", "");
                    string filter = string.Concat(_filterPath, @"\", filterStrongName, ".gcf");

                    if (File.Exists(filter))
                    {
                        ocr.IDCaptureRequest.ClassificationFilterName = filterStrongName; // used color filter
                        ocr.IDCaptureRequest.ColorFiltersPath = _filterPath; // filter directory

                        OnSetFilter(filter);
                    }
                }

                ocr.Priority = 1;

                FulltextProcessor.CurrentOCREngine.AddObjectToProcess(ocr, FulltextProcessor.CurrentProcessor);

                //wait async finish
                FulltextProcessor.CurrentOCREngine.WaitForAllToBeProcessed();

                if (ocr.ProcessedType == Gradient.MultiObjectsProcessor.ProcessedType.ProcessingError)
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine(ocr.Exception.ToString());
                }

                if (ocr.ProcessedType == Gradient.MultiObjectsProcessor.ProcessedType.Finished && ocr.IDCaptureResultData != null)
                {
                    Regex regex = new Regex(this.DetectRegex);
                    var docNo = ocr.IDCaptureResultData.Items.SingleOrDefault(i => i.Name == this.DetectKey);
                    if (docNo != null && regex.IsMatch(docNo.TextValue))
                    {
                        //save modified image
                        //IOSupport.RepeatingSaveAllBytes(_templatePath + "modified.tif", obj.IDCaptureResultData.ResultImageData);

                        foreach (var item in ocr.IDCaptureResultData.Items)
                        {
                            dic.Add(item.Name, item.Value);
                        }

                        OnRecognizeDataByTemplate(DateTime.Now - timestamp, template);

                        return dic;
                    }
                }
            }
            catch (Exception ex)
            {
                OnError(ex);
            }

            return dic;
        }

        #endregion
    }
}
