using System.Collections.Generic;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.ServiceModel.Web;

namespace Thinktecture.Samples
{
    [ServiceContract(Name = "ServiceContract", Namespace = "urn:thinktecture:samples")]
    public interface IService
    {
        [OperationContract(Name = "GetClientIdentity", Action = "GetClientIdentity", ReplyAction = "GetClientIdentityReply")]
        ViewClaims GetClientIdentity();
    }

    [ServiceContract(Name = "ServiceContract", Namespace = "urn:thinktecture:samples")]
    public interface IRestService
    {
        [OperationContract(Name = "GetClientIdentity", Action = "GetClientIdentity", ReplyAction = "GetClientIdentityReply")]
        [WebGet(UriTemplate = "/identity", ResponseFormat = WebMessageFormat.Json)]
        ViewClaims GetClientIdentity();

        [OperationContract(Name = "GetInfo", Action = "GetInfo", ReplyAction = "GetInfo")]
        [WebGet(UriTemplate = "/info", ResponseFormat = WebMessageFormat.Json)]
        string GetInfo();
    }

    [CollectionDataContract(Name = "ViewClaims", Namespace = "urn:tt", ItemName = "ViewClaim")]
    public class ViewClaims : List<ViewClaim>
    {
        public ViewClaims()
        { }

        public ViewClaims(IEnumerable<ViewClaim> claims)
            : base(claims)
        { }
    }


    [DataContract(Name = "ViewClaim", Namespace = "urn:thinktecture:samples")]
    public class ViewClaim
    {
        [DataMember]
        public string ClaimType { get; set; }

        [DataMember]
        public string Value { get; set; }

        [DataMember]
        public string Issuer { get; set; }

        [DataMember]
        public string OriginalIssuer { get; set; }
    }
}