using System;
using System.Runtime.Serialization;
using VentsCadLibrary;

namespace VentsCadServiceLibrary
{
    // NOTE: You can use the "Rename" command on the "Refactor" menu to change the class name "Service1" in both code and config file together.
    public class VentsService : IVentsCadService
    {
        public string GetData(int value)
        {
            return string.Format("You entered: {0}", value);
        }

        

        public CompositeType GetDataUsingDataContract(CompositeType composite)
        {
            if (composite == null)
            {
                throw new ArgumentNullException("composite");
            }
            if (composite.BoolValue)
            {
                composite.StringValue += "Suffix";
            }
            return composite;
        }

        public void BuildSpigot(string type, string width, string height, out VentsCad.ProductPlace place)
        {
            throw new NotImplementedException();
        }
               
        public VentsCad.Spigot Spigot(string type, string width, string height)  
        {
            VentsCad.Spigot spigot = null;
            try
            {
                using (var server = new VentsCad())
                {
                    spigot = new VentsCad.Spigot(type, width, height);
                }
            }
            catch (Exception ex)
            {
                //MessageBox.Show(ex.Message);
            }

            return spigot;
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
                //MessageBox.Show(ex.Message);
            }
        }
    }
}
