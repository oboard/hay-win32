using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Xml;

namespace HayWPF
{
    /// <summary>
    /// HayView.xaml 的交互逻辑
    /// </summary>
    public partial class HayView : UserControl
    {
        private string mUrl = "";
        private string mTitle = "";
        private List<XmlAttributeCollection> mMeta;

        public HayView()
        {
            InitializeComponent();
        }

        private void HayView_Load(object sender, EventArgs e)
        {

        }

        public async void LoadUrl(string url)
        {
            try
            {
                mUrl = url;
                string html = "";
                if (url.StartsWith("file:///"))
                    html = File.ReadAllText(url.Substring(8));
                else
                {
                    HttpClient client = new HttpClient();
                    html = await client.GetStringAsync(url);
                }
                root.Children.Clear();
                ReadHtml(root, html);
            }
            catch(Exception e)
            {
                root.Children.Clear();
                AddP(root, e.Message);
            }
        }

        public void ReadHtml(Panel panel, string html)
        {
            if (!html.Contains("<html"))
            {
                AddP(panel, html);
                return;
            }
            XmlDocument doc = new XmlDocument();
            doc.LoadXml(html);

            //meta
            mMeta = new List<XmlAttributeCollection>();
            XmlNodeList nodeList = doc.SelectNodes("html/head/meta");
            if (nodeList != null)
            {
                foreach (XmlNode resultNode in nodeList)
                {
                    XmlAttributeCollection childAttributeList = resultNode.Attributes;
                    //foreach (XmlNode attr in childAttributeList)
                    //{
                    //}
                    mMeta.Add(childAttributeList);
                }
            }

            //标题
            nodeList = doc.SelectNodes("html/head/title");
            if (nodeList != null)
            {
                foreach (XmlNode resultNode in nodeList)
                {
                    mTitle = resultNode.InnerText;
                }
            }

            nodeList = doc.SelectNodes("html/body");
            if (nodeList != null)
            {
                foreach (XmlNode resultNode in nodeList)
                {
                    XmlNodeList childNodeList = resultNode.ChildNodes;
                    foreach (XmlNode node in childNodeList)
                    {
                        XmlAttributeCollection childAttributeList = resultNode.Attributes;
                        switch (node.Name)
                        {
                            case "p":
                                AddP(panel, node.InnerText);
                                break;
                            case "img":
                                AddIMG(panel, new Uri(new Uri(mUrl), new Uri(node.Attributes["src"].Value)));
                                break;
                        }
                    }
                    //foreach (XmlNode attr in childAttributeList)
                    //{
                    //}
                }
            }
        }

        public void AddIMG(Panel panel, Uri url)
        {
            BitmapSource bitmapSource = new BitmapImage(new Uri(url.ToString(), UriKind.RelativeOrAbsolute));
            root.Children.Add(new Image
            {
                Source = bitmapSource,
                Width = bitmapSource.Width,
                Height = bitmapSource.Height
            });

        }

        public void AddP(Panel panel, string text)
        {
            root.Children.Add(new TextBlock
            {
                Text = text,

            });
            
        }

        public string GetTitle()
        {
            return mTitle;
        }
    }
}
