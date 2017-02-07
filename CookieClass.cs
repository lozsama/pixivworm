using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Pixivworm
{
    class CookieClass
    {
        private CookieClass() { }
        
        public static CookieClass Instance
        {
            get
            {
                if (Nested.instance != null)
                {
                    if (Nested.instance.cookie == null)
                    {
                        Nested.instance.cookie = new System.Net.CookieContainer();
                    }
                }
                return Nested.instance;  
            }
        }
        private class Nested
        {
            static Nested() { }
            internal static readonly CookieClass instance = new CookieClass();
        }



        public System.Net.CookieContainer cookie = new System.Net.CookieContainer();
        


    }
}
