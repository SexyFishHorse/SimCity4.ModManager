using System.IO;
using System.Reflection;
using System.Windows.Forms;

namespace SimCity4.ModManager.App.Application.View;

public partial class AboutBox : Form
{
    public AboutBox()
    {
        InitializeComponent();
        SetTextAndLabels();
    }

    public string AssemblyVersion => Assembly.GetExecutingAssembly().GetName().Version.ToString();

    private string AssemblyCompany
    {
        get
        {
            var attributes = Assembly.GetExecutingAssembly()
                .GetCustomAttributes(typeof(AssemblyCompanyAttribute), false);
            if (attributes.Length == 0)
            {
                return string.Empty;
            }

            return ((AssemblyCompanyAttribute)attributes[0]).Company;
        }
    }

    private string AssemblyCopyright
    {
        get
        {
            var attributes = Assembly.GetExecutingAssembly()
                .GetCustomAttributes(typeof(AssemblyCopyrightAttribute), false);
            if (attributes.Length == 0)
            {
                return string.Empty;
            }

            return ((AssemblyCopyrightAttribute)attributes[0]).Copyright;
        }
    }

    private string AssemblyDescription
    {
        get
        {
            var attributes = Assembly.GetExecutingAssembly()
                .GetCustomAttributes(typeof(AssemblyDescriptionAttribute), false);
            if (attributes.Length == 0)
            {
                return string.Empty;
            }

            return ((AssemblyDescriptionAttribute)attributes[0]).Description;
        }
    }

    private string AssemblyProduct
    {
        get
        {
            var attributes = Assembly.GetExecutingAssembly()
                .GetCustomAttributes(typeof(AssemblyProductAttribute), false);
            if (attributes.Length == 0)
            {
                return string.Empty;
            }

            return ((AssemblyProductAttribute)attributes[0]).Product;
        }
    }

    private string AssemblyTitle
    {
        get
        {
            object[] attributes = Assembly.GetExecutingAssembly()
                .GetCustomAttributes(typeof(AssemblyTitleAttribute), false);
            if (attributes.Length > 0)
            {
                AssemblyTitleAttribute titleAttribute = (AssemblyTitleAttribute)attributes[0];
                if (titleAttribute.Title != string.Empty)
                {
                    return titleAttribute.Title;
                }
            }

            return Path.GetFileNameWithoutExtension(Assembly.GetExecutingAssembly().CodeBase);
        }
    }

    private void SetTextAndLabels()
    {
        Text = $"About {AssemblyTitle}";
        labelProductName.Text = AssemblyProduct;
        labelVersion.Text =
            $"Version {Assembly.GetExecutingAssembly().GetName().Version}";
        labelCopyright.Text = AssemblyCopyright;
        labelCompanyName.Text = AssemblyCompany;
        textBoxDescription.Text = AssemblyDescription;
    }
}