using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using System.Collections.Specialized;

namespace wzSync.Manager
{
    class XMLGenerator
    {
        private XmlDocument xmlDoc = null;
        private XmlNode createdNode = null;
        private XmlElement createdElement = null;

        public XMLGenerator()
        {
            xmlDoc = new XmlDocument();
            // 문서를 만들고 지정된 값의 노드를 만든다..
            xmlDoc.AppendChild(xmlDoc.CreateXmlDeclaration("1.0", "utf-8", "yes"));
            // 최상위 노드를 만든다.
            XmlNode root = xmlDoc.CreateElement("", "Sync", "");
            xmlDoc.AppendChild(root);
            // 지정된 XML문서로 만들고 저장한다.
            //NewXmlDoc.Save("bookconfig.xml");
        }

        public void XML_StartCreatedFileList( int count )
        {
            if( xmlDoc != null )
            {
                createdNode = xmlDoc.DocumentElement;
                createdElement = xmlDoc.CreateElement("CreatedFileList");
                createdElement.SetAttribute("NUMBER", count.ToString());
                createdNode.AppendChild(createdElement);
            }
        }

        public void XML_AppendCreatedFile(System.IO.FileInfo file)
        {
            if (createdNode != null)
            {
                XmlElement elem = xmlDoc.CreateElement("FileItem");

                elem.AppendChild(CreateNode(xmlDoc, "Path", file.FullName));
                elem.AppendChild(CreateNode(xmlDoc, "Size", file.Length.ToString()));

                createdElement.AppendChild(elem);
            }
            else
            {
                System.Diagnostics.Debug.WriteLine("XML document is not initialized.");
            }
        }

        public void XML_EndCreatedFileList()
        {
            xmlDoc.AppendChild(createdNode);

            System.Diagnostics.Debug.WriteLine(xmlDoc.InnerXml);
        }

        public XmlNode CreateNode(XmlDocument xmlDoc, string name, string innerXml)
        {
            string temp = innerXml;
            temp = temp.Replace("&", "&amp;");
            temp = temp.Replace("<", "&lt;");
            temp = temp.Replace(">", "&gt;");
            XmlNode node = xmlDoc.CreateElement(string.Empty, name, string.Empty);
            node.InnerXml = temp;
            return node;
        }

        public void XMLModifier()
        {
            // XML문서를 불러온다
            XmlDocument XmlDoc = new XmlDocument();
            XmlDoc.Load("bookconfig.xml");
            // 첫노드를 잡아주고 하위 노드를 선택한다
            XmlNode FristNode = XmlDoc.DocumentElement;
            XmlElement SubNode = (XmlElement)FristNode.SelectSingleNode("BOOK");
            // 하위 노드 특성에 날짜를 입력하기를 원할때(추가를 원할때)
            SubNode.SetAttribute("DATA", DateTime.Today.ToString());
            // 하위 노드를 추가, 삭제, 수정하고 싶을때(BOOK보다 하위)
            // 아래 두줄은 삭제할때나, 수정할때 사용하면 된다.
            XmlNode DeleteNode = SubNode.SelectSingleNode("NAME");
            SubNode.RemoveChild(DeleteNode);
            // 아래 한줄은 추가, 수정할때 사용하면 된다.
            SubNode.AppendChild(CreateNode(XmlDoc, "NAME", "바꿔라"));
            // 위 3줄 중 위2줄은 하위 노드를 삭제하는 코딩이고
            // 아래 한줄은 추가하는 코딩이다.
            // 따라서 수정할때는 먼저 삭제하고 추가해야 한다.
            // 값변경이 안되더라...되는 방법 있으면 알고 싶다 ㅠㅠ
            // 위에 했던 행위들을 바꿔준다..
            // ReplaceChild(SubNode, SubNode); 에서 () 안에 앞에 노드는 변경할 노드
            // 뒤에 노드는 변경당할 노드
            FristNode.ReplaceChild(SubNode, SubNode);
            XmlDoc.Save("bookconfig.xml");
        }
    }
}
