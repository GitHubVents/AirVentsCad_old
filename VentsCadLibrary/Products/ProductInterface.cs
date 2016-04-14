namespace VentsCadLibrary
{
    partial class VentsCad
    { 
        interface WorkWithProduct
        {
            string Name();
            string DestinationFolder();            
            void Build();
            ProductPlace GetPlace();
        }
        public class ProductPlace
        {
            public ProductPlace(string path, int idPdm, int projectId)
            {
                IdPdm = idPdm;
                ProjectId = projectId;
                Path = path;
            }
            public int IdPdm;
            public int ProjectId;
            public string Path;
        }   
    }
}
