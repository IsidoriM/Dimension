using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Configuration;
using System.Data.SqlClient;
using System.Data;
using System.Web;
using System.Globalization;
using System.Web.Hosting;
using System.IO;
using System.Reflection;

namespace MenuPinProvisioning
{
    public class MainManager
    {
        #region Fields

        public string IdFunzionalitaPassato { get; set; }

        private IEnumerable<Funzionalita> itemList { get; set; }

        private List<MenuItem> items { get; set; }

        #endregion


        public String getMenu(String ruoli, String matricolaoperatore, String accountWindows, String idFunzionalita)
        {

            //string ruoli = "AssegnazionePIN:operatore|A1850:P2715|A1850:P9473";
            //string matricolaoperatore = "E0005258";
            //HTTP_INPS_ACCOUNT_WINDOWS = lpellegrini03
            // return getMenuHTML(getMenuItems(ruoli, matricolaoperatore));

            //Logger.Append(String.Format("ruoli {0} matricolaoperatore {1} accountWindows {2}", ruoli, matricolaoperatore, accountWindows));

            if (!String.IsNullOrEmpty(idFunzionalita))
                this.IdFunzionalitaPassato = idFunzionalita;


            if (string.IsNullOrEmpty(ruoli) || string.IsNullOrEmpty(matricolaoperatore) || string.IsNullOrEmpty(accountWindows))
            {
                return String.Format("Errore generazione Menu Parametri obbligatori non validi :  ( ruoli = {0} )  ( matricolaoperatore = {1})  ( accountWindows = {2})", ruoli, matricolaoperatore, accountWindows);
            }
            else
            {
                return generaMenuHTML(getMenuItems(ruoli, matricolaoperatore, accountWindows));
            }
        }


        /******************************/

        public string generaMenuHTML(List<Funzionalita> listFunz)
        {
            try
            {


                /* inizializzo le collection */
                itemList = listFunz;

                this.items = new List<MenuItem>();

                if (!String.IsNullOrEmpty(this.IdFunzionalitaPassato))
                    initFunzionalita();
                else
                    init();

                StringBuilder htmlBuilder = new StringBuilder();

                htmlBuilder.Append(getRowLogo());

                htmlBuilder.Append("<div Style=\"margin-top:0px;\">");
                //                htmlBuilder.Append("<div Style=\"MARGIN-LEFT: 5px; MARGIN-RIGHT: 10px\">");
                htmlBuilder.Append("<div Style=\"MARGIN-LEFT: 0px; MARGIN-RIGHT: 10px\">");
                htmlBuilder.Append("<TABLE cellSpacing=0 cellPadding=0 width=\"99%\" border=0><TBODY>");
                //class=\"lineRow\" non rilevata                                                          MARGIN-BOTTOM: 10px; HEIGHT: 1px; BACKGROUND-COLOR: #f26822
                htmlBuilder.Append("<tr id=\"TrBordoSupMenu\" runat=\"server\"><TD><div id=\"bordoSup\"  style=\"MARGIN-BOTTOM: 10px; HEIGHT: 1px; BACKGROUND-COLOR: #f26822\" runat=\"server\" ></div></td></tr>");
                htmlBuilder.Append("<tr id=\"TrMenuItems\" runat=\"server\" >");

                htmlBuilder.Append("<td>");
                htmlBuilder.Append("<DIV id=\"DivMenu\" role=\"navigation\">");

                htmlBuilder.Append("<div id='menuWrapper'>");
                htmlBuilder.Append("<ul class='menu'>");

                // Per ogni funzionalità crea eventuali sottomenu.
                foreach (var menuItem in items)
                {
                    htmlBuilder.Append(buildSubMenus(menuItem));
                }
                htmlBuilder.Append("</ul>");
                htmlBuilder.Append("</div>");//menuWrapper 

                htmlBuilder.Append("</div>");//DivMenu
                htmlBuilder.Append("</td>");//td
                htmlBuilder.Append("</tr>");//tr TrMenuItems


                //riga                                                                                        MARGIN-BOTTOM: 15px; HEIGHT: 1px; MARGIN-TOP: 18px; BACKGROUND-COLOR: #f26822       
                htmlBuilder.Append("<TR id=\"TrBordoInfMenu\" runat=\"server\"><TD><div id=\"bordoInf\" style='MARGIN-BOTTOM:15px; HEIGHT: 1px; MARGIN-TOP:18px; BACKGROUND-COLOR:#f26822'  runat=\"server\" ></div></td></tr>");

                htmlBuilder.Append("</TABLE>");

                htmlBuilder.Append("</div>");
                htmlBuilder.Append("</div>");

                //Logger.Append(htmlBuilder.ToString());

                return htmlBuilder.ToString();

            }
            catch (Exception err)
            {
                return String.Format("Error Generate Menu {0}", err.Message);
            }
        }

        private String getRowLogo()
        {
            StringBuilder sb = new StringBuilder();

            //string url = HttpContext.Current.Request.Url.AbsoluteUri;
            //// http://localhost:1302/TESTERS/Default6.aspx

            //string path = HttpContext.Current.Request.Url.AbsolutePath;
            //// /TESTERS/Default6.aspx

            //string host = HttpContext.Current.Request.Url.Host;
            // localhost

            //String strPathAndQuery = HttpContext.Current.Request.Url.PathAndQuery;
            //String strPathAndQuery = "/Home/GestioneDelegatiEntratel";
            //String strUrl = HttpContext.Current.Request.Url.AbsoluteUri.Replace(strPathAndQuery, "/");

            //String pathImgLogo = String.Format("/images/menu/logo.png");
            //String pathImgHome = String.Format("/images/menu/home.png");

            // String pathImgLogo = System.Web.HttpContext.Current.Request.ApplicationPath + String.Format("/images/menu/logo.png");
            // String pathImgHome = System.Web.HttpContext.Current.Request.ApplicationPath + String.Format("/images/menu/home.png");

            String pathImgLogo = String.Format("../images/menu/logo.png");

            String pathImgHome = String.Format("../images/menu/home.png");

            //String pathImgLogo = strUrl + String.Format("images/menu/logo.png");
            //String pathImgHome = strUrl + String.Format("images/menu/home.png");


            //String strPathAndQuery = HttpContext.Current.Request.Url.AbsolutePath;
            //String strPathTogliere = "/Home/GestioneDelegatiEntratel";
            //String Tolto = strPathAndQuery.Replace(strPathTogliere, "/");
            //String strUrl = HttpContext.Current.Request.Url.AbsoluteUri.Replace(strPathAndQuery, "");

            //String Url = HttpContext.Current.Request.Url.AbsoluteUri;
            //int numero = Url.IndexOf("Home");
            //strUrl = Url.Substring(0, numero);

            //String pathImgLogo = strUrl + String.Format("images/menu/logo.png");
            //String pathImgHome = strUrl + String.Format("images/menu/home.png");

            // String pathImgLogo =  String.Format("/images/menu/logo.png");
            // String pathImgHome =  String.Format("/images/menu/home.png");

            //string appPath = HttpContext.Current.Server.MapPath("/").ToLower();
            //appPath = string.Format("/{0}", appPath.Replace(@"\", "/"));

            //String pathImgLogo = appPath + String.Format("images/menu/logo.png");
            //String pathImgHome = appPath + String.Format("images/menu/home.png");

            string Home = System.Configuration.ConfigurationManager.AppSettings["Home"];


            sb.AppendFormat("<div Style=\"margin-left:10px; margin-right:10px;\">");

            //  sb.AppendFormat("<div style=\"background-color:#ffffff;MARGIN-LEFT: 10px; MARGIN-RIGHT: 10px\">");
            sb.AppendFormat("<div id=\"DivLogo\">");
            sb.AppendFormat("<div id=\"Logo\">");
            sb.AppendFormat("<a href=\"" + Home + "\"><img src=\"" + pathImgLogo + "\" title=\"Home\" /></a>");
            //sb.AppendFormat("<a class=\"homeButton\" href=\"PinProvisioningDefault.aspx\"><img src=\"images/menu/home.png\" title=\"Home\"/></a>");
            sb.AppendFormat("<a class=\"homeButton\" href=\"" + Home + "\"><img src=\"" + pathImgHome + "\" title=\"Home\"/></a>");
            sb.AppendFormat("</div>");
            sb.AppendFormat("</div>");
            // sb.AppendFormat("</div>");
            return sb.ToString();
        }

        private void init()
        {
            MenuItem parent = null;


            foreach (var listElem in this.itemList.Where(listElem => listElem.Attivo))
            {
                foreach (MenuItem menuItem in items)
                {
                    string idToFind = listElem.IdFunzionalitaPadre.HasValue
                                           ? listElem.IdFunzionalitaPadre.Value.ToString(CultureInfo.InvariantCulture)
                                           : string.Empty;

                    if ((parent = this.FindItem(menuItem, idToFind)) != null)
                    {
                        break;
                    }
                }

                var mnuItem = new MenuItem
                {

                    Id = listElem.IdFunzionalita.ToString(CultureInfo.InvariantCulture),
                    Description = listElem.Descrizione,
                    Title = listElem.TestoAlternativo,
                    Target = listElem.Target,
                    Url = listElem.Url,
                    Parent = parent
                };


                if (parent == null)
                {
                    // Lo aggiungo al livello 0 (quello delle funzionalità)
                    this.items.Add(mnuItem);

                }
                else
                {

                    if (parent.Childrens == null)
                    {
                        parent.Childrens = new List<MenuItem>();

                    }
                    // Lo aggiungo come sottomenu
                    parent.Childrens.Add(mnuItem);
                    String strLogger = string.Format("\n\rstart -  sottomenu {0}", mnuItem.Description);
                    //Logger.Append(strLogger);

                }


            }
        }


        private void initFunzionalita()
        {
            MenuItem parent = null;


            foreach (var listElem in this.itemList.Where(listElem => listElem.Attivo))
            {
                foreach (MenuItem menuItem in items)
                {
                    string idToFind = listElem.IdFunzionalitaPadre.HasValue
                                           ? listElem.IdFunzionalitaPadre.Value.ToString(CultureInfo.InvariantCulture)
                                           : string.Empty;

                    if ((parent = this.FindItem(menuItem, idToFind)) != null)
                    {
                        break;
                    }
                }

                var mnuItem = new MenuItem
                {

                    Id = listElem.IdFunzionalita.ToString(CultureInfo.InvariantCulture),
                    Description = listElem.Descrizione,
                    Title = listElem.TestoAlternativo,
                    Target = listElem.Target,
                    Url = listElem.Url,
                    Parent = parent,
                    IdFunzionalitaPassatoMenu = this.IdFunzionalitaPassato


                };


                if (parent == null)
                {
                    // Lo aggiungo al livello 0 (quello delle funzionalità)
                    this.items.Add(mnuItem);

                }
                else
                {

                    if (parent.Childrens == null)
                    {
                        parent.Childrens = new List<MenuItem>();

                    }
                    // Lo aggiungo come sottomenu
                    parent.Childrens.Add(mnuItem);
                    String strLogger = string.Format("\n\rstart -  sottomenu {0}", mnuItem.Description);
                    //Logger.Append(strLogger);

                }


            }
        }



        private MenuItem FindItem(MenuItem root, string idToFind)
        {
            // SV: base...
            if (root.Id.Equals(idToFind, StringComparison.OrdinalIgnoreCase))
            {
                return root;
            }

            if (root.Childrens == null)
            {
                return null;
            }

            // SV: ricorsione...
            return
                root.Childrens.Select(child => this.FindItem(child, idToFind))
                    .FirstOrDefault(findetItem => findetItem != null);
        }

        /// <summary>
        ///     Construisce il codice HTML per il singolo nodo specificato.
        /// </summary>
        /// <param name="node">Nodo di interesse.</param>
        /// <returns>Una stringa con il codice HTML generato.</returns>
        private string buildSubMenus(MenuItem node)
        {
            //Logger.Append(" node: " + node.Description);
            StringBuilder strBuilder = new StringBuilder();

            // SV: Caso in cui il PARENT == null. Per i tag li e ul vanno settate le classi "top" e "sub".
            if (node.Parent == null)
            {
                strBuilder.Append("<li class=\"top\">");
                strBuilder.Append(node.ToHtmlString());

                if (node.Childrens == null)
                {
                    strBuilder.Append("</li>");

                    return strBuilder.ToString();
                }

                // SV: questa porzione di codice è eseguita solo se il nodo ha dei figli.
                strBuilder.Append("<ul class='sub'>");

                foreach (var subnode in node.Childrens)
                {
                    //Logger.Append(" subnode : " + subnode.Description);
                    strBuilder.Append(buildSubMenus(subnode));
                }

                strBuilder.Append("</ul>");

                return strBuilder.ToString();
            }

            // SV: Caso in cui PARENT <> NULL. In questo caso specifico il nodo che si sta per aggiungere
            // va impostato some sottomenu di un nodo esistente.
            strBuilder.Append("<li>");
            strBuilder.Append(node.ToHtmlString());

            // SV: se non ha ulteriori figli mi fermo qui.
            if (node.Childrens == null)
            {
                strBuilder.Append("</li>");

                return strBuilder.ToString();
            }

            // Se, invece, ha ulteriori figli, proseguo nell'indentazione del menu.
            strBuilder.Append("<ul>");

            foreach (var subnode in node.Childrens)
            {
                // SV: Navigo più in profondità nell'albero.
                strBuilder.Append(this.buildSubMenus(subnode));
            }

            strBuilder.Append("</ul>");
            strBuilder.Append("</li>");

            return strBuilder.ToString();
        }

        private List<Funzionalita> readFunzionalita(String codiceOperatore)
        {
            List<Funzionalita> funzionalita = new List<Funzionalita>();
            try
            {
                string connstring = System.Configuration.ConfigurationManager.ConnectionStrings["SicurezzaPinProvisioning"].ConnectionString;
                //Create the connection object
                using (SqlConnection conn = new SqlConnection(connstring))
                {
                    // Open the SqlConnection.
                    conn.Open();
                    //Create the SQLCommand object
                    using (SqlCommand command = new SqlCommand("spPGetFunzionalitaUser", conn) { CommandType = System.Data.CommandType.StoredProcedure })
                    {
                        //Pass the parameter values here                     

                        command.Parameters.AddWithValue("@codice", codiceOperatore);

                        using (SqlDataReader reader = command.ExecuteReader())
                        {
                            //read the data
                            while (reader.Read())
                            {

                                Funzionalita item = DBMapper.PopulateEntity<Funzionalita>(reader);
                                var urlScope = item.Url.Split(new[] { '|' }, StringSplitOptions.RemoveEmptyEntries);

                                item.Target = urlScope.Length > 1 ? urlScope[1] : string.Empty;
                                item.Url = urlScope[0];

                                funzionalita.Add(item);

                            }
                        }
                    }
                }

            }
            catch (Exception ex)
            {
                return null;
            }
            return funzionalita;
        }




        public List<Funzionalita> getMenuItems(String ruoli, String matricolaoperatore, String accountWindows)
        {

            try
            {


                List<Funzionalita> funzionalita = readFunzionalita(ruoli);

                if (!matricolaoperatore.Contains("CRM:"))
                {
                    if (!VerificaAbilitazioneOperatore(accountWindows))
                    {

                        var item =
                            funzionalita.Find(
                                e =>
                                e.Url.Equals(
                                    "GestioneRichiesteEntiOnline.aspx",
                                    StringComparison.InvariantCultureIgnoreCase));

                        if (item != null)
                        {
                            funzionalita.Remove(item);
                        }
                    }
                }

                return funzionalita;

            }
            catch (Exception ex)
            {
                throw new Exception("MenuItems: " + ex.ToString());
            }
        }





        /// <summary>
        /// Verifica se l'operatore è abilitato. Il controllo è fatto sull'utenza di dominio
        /// della macchina.
        /// </summary>
        /// <param name="utenzaDominio">Il nome dell'utenza di dominio impostata sulla macchina.</param>
        /// <returns><c>True</c> se l'utenza di domino è abilitata; altrimenti <c>False</c>.</returns>
        public bool VerificaAbilitazioneOperatore(string utenzaDominio)
        {
            String strLogger = string.Format("\n\rstart - rsCheckAbilitazioneApprovazione  : {0} ", utenzaDominio);
            //Logger.Append(strLogger);

            string connstring = System.Configuration.ConfigurationManager.ConnectionStrings["SicurezzaPinProvisioning"].ConnectionString;
            try
            {


                //Create the connection object
                using (SqlConnection conn = new SqlConnection(connstring))
                {
                    // Open the SqlConnection.
                    conn.Open();
                    //Create the SQLCommand object
                    using (SqlCommand command = new SqlCommand("rsCheckAbilitazioneApprovazione", conn) { CommandType = System.Data.CommandType.StoredProcedure })
                    {
                        //Pass the parameter values here     
                        command.Parameters.AddWithValue("@UtenteDominio", utenzaDominio);

                        // Set Output Paramater      
                        SqlParameter param = new SqlParameter();
                        param.ParameterName = "@ReturnCode";
                        param.Value = int.MaxValue;
                        param.SqlDbType = System.Data.SqlDbType.Int;
                        param.Size = 11;
                        param.Direction = ParameterDirection.Output;
                        command.Parameters.Add(param);

                        command.ExecuteNonQuery();


                        strLogger = string.Format("\n\rsCheckAbilitazioneApprovazione  : {0}  retrun: {1}", utenzaDominio, command.Parameters["@ReturnCode"].Value);
                        // Logger.Append(strLogger);

                        return Convert.ToBoolean(command.Parameters["@ReturnCode"].Value);
                    }
                }
            }
            catch (Exception ex)
            {
                strLogger = string.Format("\n\rsCheckAbilitazioneApprovazione  Exception utenzaDominio: {0}  error: {1}", utenzaDominio, ex.Message);
                // Logger.Append(strLogger);
                return false;
            }
        }
        /**/

        /*****************************/
        internal class MenuItem
        {
            #region Public Properties

            public string IdFunzionalitaPassatoMenu { get; set; }

            /// <summary>
            ///     Ottiene o imposta l'elenco degli nodi figli dell'item corrente.
            /// </summary>
            public List<MenuItem> Childrens { get; set; }

            public string Description { get; set; }

            public string Id { get; set; }

            public MenuItem Parent { get; set; }

            public string Target { get; set; }

            public string Title { get; set; }

            public string Url { get; set; }

            #endregion

            #region Public Methods and Operators

            /// <summary>
            ///     Converte l'item in una stringa con codice HTML.
            /// </summary>
            /// <returns>La rappresentazione dell'item come stringa HTML.</returns>
            public string ToHtmlString()
            {
                StringBuilder htmlBuilder = new StringBuilder();

                if (this.Parent == null)
                {
                    string url = this.Url.Replace("'", "\"");

                    if (string.IsNullOrEmpty(this.Target))
                    {
                        if (!this.Url.ToUpperInvariant().Contains("javascript:".ToUpperInvariant()))
                        {
                            url += (url.Contains("?") ? "&" : "?") + "m=" + this.Id;
                        }
                    }
                    else if (!this.Url.ToUpperInvariant().Contains("javascript:".ToUpperInvariant()))
                    {
                        url += (url.Contains("?") ? "&" : "?") + "m=" + this.Id;
                    }

                    String cssDynamic = "";
                    if (!String.IsNullOrEmpty(this.IdFunzionalitaPassatoMenu))
                        cssDynamic = getCssDynamic(HttpUtility.HtmlEncode(url), this.IdFunzionalitaPassatoMenu);
                    else
                        cssDynamic = getCssDynamic(HttpUtility.HtmlEncode(url));
                    htmlBuilder.Append(
                        "<a id='nav-" + this.Id + "' href='" + HttpUtility.HtmlEncode(url) + "' class='" + cssDynamic + "' target='"
                        + this.Target + "' title='" + HttpUtility.HtmlEncode(this.Title) + "'>");
                    htmlBuilder.Append("<span>" + HttpUtility.HtmlEncode(this.Description) + "</span>");
                    htmlBuilder.Append("</a>");
                }
                else
                {
                    // In rootNode conservo l'item radice del sottomenu corrente.
                    MenuItem rootNode = this.Parent;

                    while (rootNode.Parent != null)
                    {
                        rootNode = rootNode.Parent;
                    }

                    string url = this.Url.Replace("'", "\"");

                    if (string.IsNullOrEmpty(this.Target))
                    {
                        if (!this.Url.ToUpperInvariant().Contains("javascript:".ToUpperInvariant()))
                        {
                            url += (url.Contains("?") ? "&" : "?") + "m=" + rootNode.Id;
                        }
                    }
                    else if (!this.Url.ToUpperInvariant().Contains("javascript:".ToUpperInvariant()))
                    {
                        url += (url.Contains("?") ? "&" : "?") + "m=" + rootNode.Id;
                    }

                    // Se ha dei figli significa che ci sono ulteriori sottomenu e lo indico con la classe 'fly'.
                    if (this.Childrens != null)
                    {
                        htmlBuilder.Append(
                            "<a id='nav-" + this.Id + "' href='" + HttpUtility.HtmlEncode(url) + "' class='fly' target='"
                            + this.Target + "' title='" + HttpUtility.HtmlEncode(this.Title) + "'>");
                        htmlBuilder.Append("   <span>" + HttpUtility.HtmlEncode(this.Description) + "</span>");
                        htmlBuilder.Append("</a>");
                    }
                    else
                    {
                        htmlBuilder.Append(
                            "<a id='nav-" + this.Id + "' href='" + HttpUtility.HtmlEncode(url) + "' class='fly' target='"
                            + this.Target + "' title='" + HttpUtility.HtmlEncode(this.Title) + "'>");
                        htmlBuilder.Append("   <span>" + HttpUtility.HtmlEncode(this.Description) + "</span>");
                        htmlBuilder.Append("</a>");
                    }
                }
                return htmlBuilder.ToString();
            }

            private string getCssDynamic(string urlMenu, String paramM)
            {
                String styleDynamic = "top_link";
                Uri uriTemp = null;

                var valoreM = "";
                try
                {
                    if (paramM != null && paramM.Trim().Length > 0)
                    {

                        uriTemp = new Uri("http://" + HttpUtility.HtmlEncode(urlMenu));
                        var query = HttpUtility.ParseQueryString(uriTemp.Query);
                        if (query != null)
                        {
                            valoreM = query.Get("m");

                            if (valoreM.Trim().ToUpper() == paramM.Trim().ToUpper())
                            {
                                styleDynamic = "select top_link";
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    //Logger.Append(String.Format("Errore getCssDynamic {0} {1} {2}", ex.Message, urlMenu.ToString(), valoreM));

                }
                return styleDynamic;
            }


            private string getCssDynamic(string urlMenu)
            {
                String styleDynamic = "top_link";
                Uri uriTemp = null;

                var valoreM = "";
                try
                {
                    string paramM = System.Configuration.ConfigurationManager.AppSettings["ParamM"];
                    if (paramM != null && paramM.Trim().Length > 0)
                    {

                        uriTemp = new Uri("http://" + HttpUtility.HtmlEncode(urlMenu));
                        var query = HttpUtility.ParseQueryString(uriTemp.Query);
                        if (query != null)
                        {
                            valoreM = query.Get("m");

                            if (valoreM.Trim().ToUpper() == paramM.Trim().ToUpper())
                            {
                                styleDynamic = "select top_link";
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    //Logger.Append(String.Format("Errore getCssDynamic {0} {1} {2}", ex.Message, urlMenu.ToString(), valoreM));

                }
                return styleDynamic;
            }


            #endregion
        }
        /***************************/

    }
}
