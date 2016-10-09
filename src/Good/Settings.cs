namespace WebApplication
{
    public class Settings
    {
        public Settings()
        {
            this.Authentication = new Authentication();
        }

        public Authentication Authentication { get; set; }

        public bool EnableEmbeddedDocumentation { get; set; }

        public bool EnableRedisCache { get; set; }
    }
}