

using System.Collections.Generic;

namespace VentsCadLibrary
{
    partial class VentsCad
    {
        public class ProductFactory
        {
            public Product product;            

            public class Parameters
            {                
                public string Name { get; set; }
                
                public string Type { get; set; }

                public List<Sizes> Sizes { get; set; }

                public List<string[]> Materials { get; set; }
            }

          

            public class Sizes
            {
                public string Width { get; set; }

                public string Height { get; set; }

                public string Lenght { get; set; }
            }



            public ProductFactory(Parameters parameters)
            {
                using (var server = new VentsCad())
                {
                    switch (parameters.Name)
                    {
                        case "spigot":
                            product = new Spigot(parameters.Type, parameters.Sizes[0].Width, parameters.Sizes[0].Height);
                            break;
                        case "dumper":
                            product = new Dumper(parameters.Type, parameters.Sizes[0].Width, parameters.Sizes[0].Height, true, parameters.Materials[0]);
                            break;
                        default:
                            break;
                    }
                }
            }


            public ProductFactory(string[] query)
            {
                using (var server = new VentsCad())
                {
                    switch (query[0])
                    {
                        case "spigot":
                            product = new Spigot(query[1], query[2], query[3]);
                            break;                        
                        default:
                            break;
                    }
                }
            }         
        }
    }
}
