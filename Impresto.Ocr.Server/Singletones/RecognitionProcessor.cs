using Impresto.Ocr.Core;
using System;
using System.IO;

namespace Impresto.Ocr.Server.Singletones
{
    public sealed class RecognitionProcessor : AbstractRecognitionProcessor
    {
        private static object _sync = new object();

        public override string SdkPath
        {
            get { return Impresto.Ocr.Server.Properties.Settings.Default.SdkPath; }
        }

        public override string DetectKey
        {
            get { return Impresto.Ocr.Server.Properties.Settings.Default.DetectKey; }
        }

        public override string DetectRegex
        {
            get { return Impresto.Ocr.Server.Properties.Settings.Default.DetectRegex; }
        }

        #region Ctor

        private RecognitionProcessor()
        {
        }

        #endregion

        private static RecognitionProcessor _instance;
        public static RecognitionProcessor Instance
        {
            get 
            {
                if (_instance == null)
                {
                    lock (_sync)
                    {
                        if (_instance == null)
                        {
                            _instance = new RecognitionProcessor();
                            _instance.Init(Path.Combine(_instance.SdkPath));
                        }
                    }
                }

                return _instance;
            }
        }

        protected override void OnInit(TimeSpan timestamp)
        {
            Logger.Instance.InfoFormat("Successfully init RecognitionProcessor at time: {0}s", timestamp.TotalSeconds);
            base.OnInit(timestamp);
        }

        protected override void OnTry(string template)
        {
            Logger.Instance.InfoFormat("Try recognize data with template: {0}", template);
            base.OnTry(template);
        }

        protected override void OnSetFilter(string filter)
        {
            Logger.Instance.InfoFormat("Set filter: {0}", filter);
            base.OnSetFilter(filter);
        }

        protected override void OnRecognizeData(TimeSpan timestamp, string template)
        {
            Logger.Instance.InfoFormat("Recognize data at time: {0}s with template: {1}", timestamp.TotalSeconds, template);
            base.OnRecognizeData(timestamp, template);
        }

        protected override void OnError(Exception ex)
        {
            Logger.Instance.Error("Gradient", ex);
            base.OnError(ex);
        }
    }
}