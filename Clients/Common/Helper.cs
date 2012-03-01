using System;
using System.Diagnostics;
using Thinktecture.IdentityModel.Utility;

namespace Thinktecture.Samples
{
    public static class Helper
    {
        public static void ShowClaim(ViewClaim claim)
        {
            Console.WriteLine(claim.ClaimType);
            (" " + claim.Value).ConsoleGreen();
            Console.WriteLine(" ({0} -- {1})\n", claim.Issuer, claim.OriginalIssuer);
        }

        public static void Timer(Action a)
        {
            var sw = new Stopwatch();

            sw.Start();
            a();

            string.Format("\n\nElapsed Time: {0}", sw.ElapsedMilliseconds).ConsoleRed();
        }
    }
}
