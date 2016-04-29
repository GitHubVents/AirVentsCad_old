using System.Collections.Generic;
using System.Runtime.Serialization;
using System.ServiceModel;

namespace VentsCadServiceLibrary
{    
    [ServiceContract]
    public interface IVentsCadService
    {
        [OperationContract]
        void Build(Parameters parameters, out ProductPlace place);

        [OperationContract]
        bool IsBusy();       

        [OperationContract]
        void BuildSp(string type, string width, string height, out ProductPlace place);

        [OperationContract]
        void BuildSpigot(string type, string width, string height, out int projectId, out int idPdm);
    }

    [DataContract]
    public class Parameters
    {
        [DataMember]
        public string Name;
        [DataMember]
        public string Type;        
        [DataMember]
        public List<Sizes> Sizes;
        [DataMember]
        public List<string[]> Materials;
    }

    [DataContract]
    public class Sizes
    {
        [DataMember]
        public string Width;
        [DataMember]
        public string Height;
        [DataMember]
        public string Lenght;
    }
   
    [DataContract]
    public class ProductPlace
    {
        [DataMember]
        public int IdPdm;
        [DataMember]
        public int ProjectId;
    }

    #region Example

    // Use a data contract as illustrated in the sample below to add composite types to service operations.
    // You can add XSD files into the project. After building the project, you can directly use the data types defined there, with the namespace "VentsCadServiceLibrary.ContractType".
    [DataContract]
    public class CompositeType
    {
        bool boolValue = true;
        string stringValue = "Hello ";

        [DataMember]
        public bool BoolValue
        {
            get { return boolValue; }
            set { boolValue = value; }
        }

        [DataMember]
        public string StringValue
        {
            get { return stringValue; }
            set { stringValue = value; }
        }
    }

    #endregion
}
