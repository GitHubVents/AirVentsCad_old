using System;
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
    }
}
