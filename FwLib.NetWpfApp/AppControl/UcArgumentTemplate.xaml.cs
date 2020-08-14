using System.Windows.Controls;

namespace FwLib.NetWpfApp.AppControl
{
    /// <summary>
    /// Interaction logic for UcArgumentTemplate.xaml
    /// </summary>
    public partial class UcArgumentTemplate : UserControl
    {
        #region Private Data
        private string _argName = string.Empty;
        #endregion

        #region Public Properties
        public string ArgName
        {
            get => _argName;
        }

        public string ArgValue
        {
            get => TbArgValue.Text;
        }
        #endregion

        #region Constructors
        public UcArgumentTemplate()
        {
            InitializeComponent();
        }

        public UcArgumentTemplate(string ArgumentName)
        {
            InitializeComponent();

            _argName = ArgumentName;
            LblArgName.Content = ArgumentName;
        }
        #endregion
    }
}
