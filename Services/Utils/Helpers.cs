using System;

namespace vscodecore.Utils.Helpers {

    public static class Helpers {
        public static bool IsDevelopment() {
            return Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") == "Development";
        }
    }
}