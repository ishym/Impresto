using Impresto.Ocr.Server.Controllers;
using log4net;

namespace Impresto.Ocr.Server.Singletones
{
    public sealed class Logger
    {
        private static object _sync = new object();

        private Logger()
        {

        }

        private static ILog _instance;
        public static ILog Instance
        {
            get
            {
                if (_instance == null)
                {
                    lock (_sync)
                    {
                        if (_instance == null)
                        {
                            _instance = LogManager.GetLogger(typeof(RecognizeController));
                            log4net.Config.XmlConfigurator.Configure();
                        }
                    }
                }

                return _instance;
            }
        }
    }

}