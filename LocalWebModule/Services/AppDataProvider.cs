using LocalJsonModule.Services;

namespace LocalWebModule.Services
{
    public class AppDataProvider : IDataPathProvider
    {
        public string DataPath { get; }

        public AppDataProvider(IHostEnvironment env) 
        {
            var basePath = Path.Combine(env.ContentRootPath, "Data");
            this.DataPath = basePath;

            if (!Directory.Exists(basePath))
            {
                Directory.CreateDirectory(basePath);
            }
            
        }
    }
}
