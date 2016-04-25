using System;
using System.Collections.Generic;
using System.Windows.Forms;
using VentsCadLibrary;

namespace VentsCadServiceLibrary
{    
    public class VentsService : IVentsCadService
    {  
        public void BuildSp(string type, string width, string height, out ProductPlace place)
        {
            busy = true;
            var projectId = 0;
            var idPdm = 0;
            BuildSpigot(type, width, height, out projectId, out idPdm);
            place = new ProductPlace
            {
                IdPdm = idPdm,
                ProjectId = projectId
            };            
        }

        public void Build(Parameters parameters, out ProductPlace place)
        {
            VentsCad.ProductFactory serviceObj = new VentsCad.ProductFactory(new VentsCad.ProductFactory.Parameters
            {
              Name = parameters.Name,
              Type = parameters.Type,
              Sizes = new List<VentsCad.ProductFactory.Sizes>
              {
                  new VentsCad.ProductFactory.Sizes
                  {
                      Width = parameters.Sizes[0]?.Width,
                      Height = parameters.Sizes[0]?.Height,
                      Lenght = parameters.Sizes[0]?.Lenght
                  }
              },
              Materials = new List<string[]> { parameters.Materials[0]  }
              
            });

            MessageBox.Show(serviceObj.product.Exist.ToString(), "idPdm - " + serviceObj.product.Place?.IdPdm.ToString());
            serviceObj.product.Build();

            var getPlace = serviceObj.product.GetPlace();

            place = new ProductPlace
            {
                IdPdm = getPlace.IdPdm,
                ProjectId = getPlace.ProjectId
            };
        }


        public void BuildSpigot(string type, string width, string height, out int projectId, out int idPdm)
        {
            projectId = 0;
            idPdm = 0;
            try
            {
                using (var server = new VentsCad())
                {
                    var newSpigot = new VentsCad.Spigot(type, width, height);
                    if (!newSpigot.Exist)
                    {
                        newSpigot.Build();
                    }
                    var place = newSpigot.GetPlace();

                    projectId = place.ProjectId;
                    idPdm = place.IdPdm;
                }
            }
            catch (Exception ex)
            {               
                MessageBox.Show(ex.Message);
            }
            busy = false;
        }       

        public bool IsBusy()
        {
            return busy;
        }

        internal bool busy { get; set; }
    }
}
