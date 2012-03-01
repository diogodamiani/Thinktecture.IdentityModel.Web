using System.IdentityModel.Tokens;
using System.ServiceModel;
using System.Text;
using System.Windows;
using Microsoft.IdentityModel.Protocols.WSTrust;
using Thinktecture.IdentityModel.Web;
using Thinktecture.Samples;

namespace WebIdentityWpfClient
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public JsonNotifyRequestSecurityTokenResponse RSTR { get; set; }

        public MainWindow()
        {
            InitializeComponent();
        }

        private void _btnSignin_Click_1(object sender, RoutedEventArgs e)
        {
            var dialog = new SignInWindow()
            {
                AcsNamespace = "ttacssample",
                Realm = "https://" + Constants.WebHost + "/webservicesecurity/",
                Owner = this
            };

            if (dialog.ShowDialog().Value)
            {
                RSTR = dialog.Response;
                _txtDebug.Text = RSTR.SecurityTokenString;
            }
        }

        private void _btnCallService_Click(object sender, RoutedEventArgs e)
        {
            var binding = new WS2007FederationHttpBinding(WSFederationHttpSecurityMode.TransportWithMessageCredential);
            binding.Security.Message.EstablishSecurityContext = false;
            binding.Security.Message.IssuedKeyType = SecurityKeyType.BearerKey;

            var ep = new EndpointAddress("https://" + Constants.WebHost + "/webservicesecurity/soap.svc/bearer");

            var factory = new ChannelFactory<IService>(binding, ep);
            factory.Credentials.SupportInteractive = false;
            factory.ConfigureChannelFactory();

            var channel = factory.CreateChannelWithIssuedToken(RSTR.SecurityToken);
            var claims = channel.GetClientIdentity();
            
            var sb = new StringBuilder(128);
            claims.ForEach(c => sb.AppendFormat("{0}\n {1}\n\n", c.ClaimType, c.Value));
            _txtDebug.Text = sb.ToString();
        }
    }
}
