namespace AccountsChu.API.Common
{
    public static class AppExtension
    {
        public static void ConfigureDevEnvironment(this WebApplication app)
        {
            app.UseSwagger();
            app.UseSwaggerUI();
        }

        public static void UseSecurity(this WebApplication app)
        {

        }
    }
}
