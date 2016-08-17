using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OpenTK;

namespace ProjectorGaletes
{
    class Program
    {
        static void Main(string[] args)
        {
            Game game = new Game(1280, 980);
            game.Title = "ProjBob";
            game.VSync = VSyncMode.On; // to not measure performance

         //   try
         //   {
                game.Run(60.0, 60.0);
         //   }
         //   catch (Exception)
         //   {

         //       throw;
         //       return;
         //   }
        }
    }
}
