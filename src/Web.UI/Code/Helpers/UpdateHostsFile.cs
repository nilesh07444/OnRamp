using System;
using System.IO;

namespace Web.UI.Code.Helpers
{
    public static class UpdateHostsFile
    {
        public static void UpdateHostsFileOnSystem(string subDomain)
        {
            try
            {
                if (IsDnsByPassed(subDomain))
                {
                    using (
                        StreamWriter w =
                            File.AppendText(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.System),
                                "drivers/etc/hosts")))
                    {
                        w.WriteLine("127.0.0.1  " + subDomain);
                    }
                }
            }
            catch (Exception)
            {
                throw;
            }
        }

        private static bool IsDnsByPassed(string subDomain)
        {
            string path = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.System), @"drivers\etc\hosts");
            string hostsText = File.ReadAllText(path);
            return hostsText.ToLower().Contains(subDomain.ToLower());
        }
    }
}