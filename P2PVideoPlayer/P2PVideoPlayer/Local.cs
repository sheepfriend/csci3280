using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace WpfApplication1
{
    class Local
    {
        public static String ref_addr = "";
        public static bool exist(String file)
        {
            return System.IO.File.Exists(ref_addr+ file);
        }
        public static bool writeFile(String file, byte[] data)
        {
            if (exist(file)) { return false; }
            else
            {
                try
                {
                    System.IO.File.WriteAllBytes(ref_addr + file, data);
                    return true;
                }
                catch
                {
                    return false;
                }
            }
        }

    }
}
