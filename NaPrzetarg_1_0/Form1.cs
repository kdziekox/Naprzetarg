using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.IO;
using System.Diagnostics;
using System.Xml;
using System.Xml.Linq;
using System.Data.Sql;
using MySql.Data.MySqlClient;
using System.Text.RegularExpressions;


namespace NaPrzetarg_1_0
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
           Load += button6_Click;//Register the event handler so that it will work for you.
          //  this.Load += new System.EventHandler(this.button6_Click);
          Dictionary<string, string> selectedType =new Dictionary<string, string>
            {
              {"Wszystkie", ">'1'"},
              {"Dostawy", "='D'"},
              {"Usługi", "='U'"},
              {"Budowlane", "='B'"}
            };
           comboBox3.DataSource = new BindingSource(selectedType, null);
           comboBox3.DisplayMember = "Key";
           comboBox3.ValueMember = "Value";
        Dictionary<string, string> selectedDepartment =new Dictionary<string, string>
            {
              {"Wszystkie", ">'1'"},
              {"dolnośląskie", "LIKE'dolnoś%'"},
              {"kujawsko-pom", "LIKE'kujawsko%'"},
              {"lubelskie", "LIKE'lubelski%'"},
              {"lubuskie", "LIKE'lubuski%'"},
              {"łódzkie", "LIKE'łódzk%'"},
              {"małopolskie", "LIKE'małopolski%'"},
              {"mazowieckie", "LIKE'mazowiecki%'"},
              {"opolskie", "LIKE'opolskie%'"},
              {"podkarpackie", "LIKE'podkarpacki%'"},
              {"podlaskie", "LIKE'podl%'"},
              {"pomorskie", "LIKE'pom%'"},
              {"śląskie", "LIKE'śląski%'"},
              {"świętokrzyskie", "LIKE'święto%'"},
              {"warmińsko-mazurskie", "LIKE'warm%'"},
              {"wielkopolskie", "LIKE'wielko%'"},
              {"zachodniopomorskie", "LIKE'zacho%'"},
              
            };
           comboBox4.DataSource = new BindingSource(selectedDepartment, null);
           comboBox4.DisplayMember = "Key";
           comboBox4.ValueMember = "Value";
        }

        public void logBox (string action)
        {
            
            logBox1.Items.Insert(0, DateTime.Now.ToString()+": "+action);
            //logBox1.Items.Add(action);
            logBox1.Refresh();
        }


        public void createDir(string name)
        {
            if (!Directory.Exists(name))
            {
                Directory.CreateDirectory(name);
                logBox("Creating directory " + name);
            }
        }
        public void extractFile(string folder, string file)
        {
            if (File.Exists(folder + file))
            {
                logBox("extracting file "+ file +"at "+ folder);
                FileInfo fi = new FileInfo(folder+ file);
                Process extract = new Process();
                extract.StartInfo.FileName = folder + file;
                extract.StartInfo.WorkingDirectory = folder;
                extract.StartInfo.CreateNoWindow = false;
                extract.Start();
                extract.WaitForExit();
            }
        }
        

        public Boolean checkFileExistances(string pozycja,int ZPtype)
        {
            Dictionary<string, string> addedRecords = new Dictionary<string, string>();

            //tylko sprawdzanie.
            //dodawanie plików powinno odbywać sie po dodaniu fizycznym 
            if (File.Exists(@".//addedRecords.txt"))
            {
                string[] fileLines = System.IO.File.ReadAllLines(@".//addedRecords.txt");
                string[] stringSeparator = new string[] { "|" };

                //Wczytanie calego pliku
                foreach (string dataLine in fileLines)
                {
                    string[] fileData = dataLine.Split(stringSeparator, StringSplitOptions.None);
                    addedRecords.Add(fileData[0], fileData[1]);
                }

                if (addedRecords.ContainsKey(pozycja))
                {
                    return false;
                
                }
                else
                {

                    using (System.IO.StreamWriter file = new System.IO.StreamWriter(@".//addedRecords.txt", true))
                    file.WriteLine(pozycja + "|" + ZPtype.ToString());
                    return true;
                    
                }

            }
            else 
            {

                using (System.IO.StreamWriter file = new System.IO.StreamWriter(@".//addedRecords.txt", true))
                    file.WriteLine(pozycja + "|" + ZPtype.ToString());
                    
                return false;
            }
                        
        }
        public void connectFTP()
        {
            logBox("connecting FTP");
            string FtpServer = "ftp.uzp.gov.pl";
            string FtpUser = "anonymous";
            string FtpPassword = "";
           
            WebCamService.FtpClient ftp = new WebCamService.FtpClient(FtpServer, FtpUser, FtpPassword);
            ftp.Login();
            logBox("connected to ftp");
            ftp.ChangeDir("bzp/dzis/xml/");
            DateTime data = DateTime.Now;
            string dataFormated = data.Year.ToString("D") + data.Month.ToString("D2") + data.Day.ToString("D2");
            string[] plikiFTP = ftp.GetFileList();
            int ilosc = plikiFTP.Length;
            string m = plikiFTP[0];
            string newDir = @"C:\XMLfiles\" + dataFormated;
            createDir(newDir );
            logBox("dowloading file dzisxml.exe");
            ftp.Download("dzisxml.exe", newDir + "\\dzisxml.exe");
            logBox("Koniec ściągania trwa rozpakowywanie.");
            extractFile(newDir, "\\dzisxml.exe");
        }

        public void connectDatabase()
        {
            logBox("connecting database");
        }
        public void logFiles(string folder)
        {
            //TBD            
            logBox("starting log file update");
            string[] tablicaPlikow = Directory.GetFiles(folder, "*.xml");
                    
        }

        //string numerOglo= "400";
        //string numerOglo = comboBox2.Text.ToString();

        public void updateTables()
        {
            string numerOglo = comboBox2.Text.ToString();
            //to be changed  selecting from directory
            DateTime data = DateTime.Now;
            string dataFormated = data.Year.ToString("D") + data.Month.ToString("D2") + data.Day.ToString("D2");
            string folder = @"C:\XMLfiles\" + dataFormated;
            //string folder = @"e:\przetargi2015\2015";
            
            DataSet baza = new DataSet();
            baza.Tables.Add(new DataTable("Zam"+numerOglo));

            
            string[] tablicaPlikow = Directory.GetFiles(folder, "*.xml");

            // jest problem ze znakiem & np w C:\XMLfiles\20140610\125308_2014.xml
            foreach (string tmp in tablicaPlikow)
            {
                XmlDocument xdc = new XmlDocument();
                xdc.Load(tmp);
                if (xdc.DocumentElement.Name == "ZP-" + numerOglo)
                {
                    XmlNodeList listaObiektow = xdc.DocumentElement.ChildNodes;

                    Dictionary<string, string> kontener = new Dictionary<string, string>();

                    foreach (XmlNode xn in listaObiektow)
                    {

                        if (xn.Name != "czesci" && xn.FirstChild != null)
                        {
                            kontener.Add(xn.Name, xn.FirstChild.Value.Replace("|",";").Replace("'",";") );
                        }

                        if (!baza.Tables["Zam" + numerOglo].Columns.Contains(xn.Name) && xn.Name != "czesci")
                        {
                            baza.Tables["Zam" + numerOglo].Columns.Add(xn.Name);
                        }

                        
                        
                        
                        
                        if (xn.Name == "czesci" && xn.FirstChild != null)
                        {

                             string[] stringSeparators = new string[] {"[cz_0]"};
     
                            string stringWithXml = xn.InnerXml;
                            XmlNode node1 = xn.NextSibling;
                            string stringWithXml1 = xn.Name;
                            XmlNodeList Lista1 = xn.ChildNodes;
                            XmlNode node2 = xn.FirstChild;
                            bool czymanode = xn.HasChildNodes;
                            XmlNode node3 = xn.LastChild;
                            XmlDocument doc1 = xn.OwnerDocument;
                            string prefix2 = xn.Prefix;

                            string content = xn.InnerXml;
                            string parseParts="<cz_";
                            string parseExecutor = "<wykonawca_";

                            int numberOfParts = Regex.Matches(content, parseParts).Count;
                            int numberOfExecutor = Regex.Matches(content, parseExecutor).Count;
                            numberOfExecutor = numberOfExecutor / numberOfParts; //każda część ma przynajmniej jednego 
                           // int count=xn.InnerXml.Count(f => f == 'cz_');
                           
                            for (int i=0; i<numberOfParts; i++)
                                
                            {
                                string selection = "/ZP-" + numerOglo + "/czesci/cz_" + i ; //+ "/wykonawcy/wykonawca_0";
                                XmlNodeList xnListPart = xn.SelectNodes(selection);

                           //     XmlNodeList listaObiektow1 = xn.DocumentElement.ChildNodes;

                                foreach (XmlNode xnn in xnListPart)
                                {
                                    string parts1 = xnn.InnerXml;
                                    string parts = xnn.OuterXml;
                                    string part3 = content;
                                     XmlDocument wykonawcy = new XmlDocument();
                                     
                                    wykonawcy.LoadXml(parts);
                                    XmlNodeList listaObiektow3 = wykonawcy.DocumentElement.ChildNodes;


                                    foreach (XmlNode wy in listaObiektow3)
                                     {
                                        // kontener.Add(xnn.Name + "cz_" + i, xnn.FirstChild.Value.Replace("|", ";").Replace("'", ";"));
                                         
                                             string name = wy.Name + "_cz_" + i;
                                             string value = wy.FirstChild.InnerText.Replace("|", ";").Replace("'", ";");

                                             kontener.Add(name, value);

                                        //   kontener.Add(wy.Name + "cz_" + i, wy.FirstChild.Value.Replace("|", ";").Replace("'", ";"));
                                         
                                        
                                        // string name1 = wy.Name + "cz_" + i;
                                        // string name11 = wy.FirstChild.Value;
                                        // string name12 = wy.Name + "cz_" + i;
                                        // string name2 = wy.FirstChild.Value.Replace("|", ";").Replace("'", ";");
                                        // kontener.Add(wy.Name + "cz_" + i, wy.FirstChild.Value.Replace("|", ";").Replace("'", ";"));
                                        // string parts = xnn.InnerXml;
                                     }
                             }

                            }
                           
                            
                            XmlNodeList xnList = xn.SelectNodes("/ZP-403/czesci/cz_0/wykonawcy/wykonawca_0");
                           

                            
                            foreach (XmlNode xnn in xnList)
                            

                            {
                                string wyk = xnn.InnerXml;

                              //  string firstName = xnn["wykonawca_0"].InnerText;
                               // string lastName = xnn["LastName"].InnerText;
                                //Console.WriteLine("Name: {0} {1}", firstName, lastName);
                            }


                            //string[] split = xn.InnerXml.Split(new Char[] { ' ', ',', '.', ':', '\t' });


                        //    xn.OuterXml
                       
                  
                        }

                                          
                    }



                    #region

                    // prepare string for db 

                    string kolumna ="";
                    string kontent ="";
                    List<string> list = new List<string>(kontener.Keys);
                    // Loop through list
                    //regexpem  zamienić wszyskie znaki typu ""||'' z przedmiotu zamówienia.done
                    //dorzucić sprawdzanie w pliku długości characterów. done 
                    //oraz dopisywanie do bazy nowych pól. done
                    //upbate bazy 
                    //sprawdzenie dat
                    //plik z "pozycja" done
                    //Stworzenie bazy z zebranymi wartościami.
                  
                    Dictionary<string, string> databaseTables = new Dictionary<string, string>();

                    if (File.Exists(@".\Baza"+numerOglo+".txt"))
                    {
                        string[] fileLines = System.IO.File.ReadAllLines(@".//baza" + numerOglo + ".txt");
                        string[] stringSeparator = new string[] {"|"};
      
                       //Wczytanie calego pliku
                        foreach( string dataLine in fileLines)
                        {
                            string[] fileData = dataLine.Split(stringSeparator, StringSplitOptions.None);
                            databaseTables.Add(fileData[0],fileData[1]);
                       
                        }
     
                    }

                    {
                        foreach (string line in list)
                        {

                            kolumna = kolumna + line;
                            kontent = kontent + "'" + kontener[line] + "'";
                            
                          
                            //Check longitude of table rows 
                            if (databaseTables.ContainsKey(line))
                            {
                                int a = kontener[line].Length ;// dane z nowego pliku xml
                               var a1 = databaseTables[line];
                                int b=Convert.ToInt32(databaseTables[line]);// dane z historycznego pliku.

                                if (a > b)
                                {
                                    //replaced value when larger data.
                                    // database update necesery
                                    string testName = line + "|" + a;
                                    File.WriteAllText(@".//baza" + numerOglo + ".txt", File.ReadAllText(@".//baza" + numerOglo + ".txt").Replace(line + "|" + b, line + "|" + a));
                                    logBox("changed "+line+" to " + databaseTables[line] );
                                    alterTableModifyColumn(numerOglo, line, "VARCHAR", a);
                                }

                            }
                            else
                            {
                                using (System.IO.StreamWriter file = new System.IO.StreamWriter(@".//baza" + numerOglo + ".txt", true))
                  
                                file.WriteLine(line + "|" + kontener[line].Length);
                                alterTableAddColumn(numerOglo, line, "VARCHAR", kontener[line].Length);
                            
                            }
                        }
                    }

                    Dictionary<string, string> databaseCall = new Dictionary<string, string>();
                    int dbLineCount = 0;
                    foreach (string line in databaseTables.Keys)
                        {
                        dbLineCount = dbLineCount + 1;
                            if (kontener.Keys.Contains(line))
                            {
                                databaseCall.Add(line, kontener[line]);
                            }
                            else
                            {
                                databaseCall.Add(line, "");
                            }
                        if (dbLineCount == 200)

                            break;
                    } 
                          
                       
                    #endregion 


                    logBox(" Item ready to DB add");


                    StringBuilder rowNames = new StringBuilder();
                    StringBuilder rowValues = new StringBuilder();
                    rowValues.Append("'");
                    int lineCount = 0;

                    foreach (string line in databaseCall.Keys)
                    {
                        string tex = databaseTables[line];
                        rowNames.Append(line);
                        rowValues.Append(databaseCall[line]);

                        if (lineCount + 1 < databaseCall.Count)
                        {
                            rowNames.Append(",");
                            rowValues.Append("','");
                        }
                        lineCount = lineCount + 1;
                        if (lineCount == 200)

                            break;
                        
                    }

                    if (checkFileExistances(kontener["pozycja"], Convert.ToInt32(numerOglo)))
                    {
                        try
                        {
                            logBox("Connecting DB");
                            logBox("Creating command");

                            string MyConString = "Server=sql.naprzetarg.pl;" +
                            //string MyConString = "Server=http://sql.naprzetarg.pl/sql/;" +
                                                                             "Database=aniaikarol;" +
                                                                             "Uid=aniaikarol;" +
                                                                             "Pwd=!NaPrzetarg;";
                            MySqlConnection connection = new MySqlConnection(MyConString);
                            MySqlCommand command = connection.CreateCommand();


                            MySqlConnection myConnection = new MySqlConnection(MyConString);
                            if (numerOglo == "402")
                                
                            {

                                logBox("402");
                            }

                    //        string myInsertQuery = "INSERT INTO large_tabele_403 (biuletyn ,pozycja ,data_publikacji ,nazwa ,ulica ,nr_domu ,miejscowosc ,kod_poczt ,wojewodztwo ,tel ,fax ,regon ,e_mail ,ogloszenie ,nrbiuletynu ,nrpozycji ,datawydaniabiuletynu , czy_obowiazkowa ,czy_biul_pub ,biul_pub_rok ,biul_pub_poz ,czy_zmiana_ogl ,dotyczy ,rodzaj_zam ,nazwa_zamowienia ,rodz_zam ,przedmiot_zam ,cpv1c ,cpv2c ,kod_trybu ,zamowienie_ue ,zal_pprawna_hid ,zal_pprawna ,zal_uzasadnienie ,rodzaj_zam_inny ,cpv3c ,cpv4c ,cpv5c ,cpv6c ,cpv7c ,cpv8c ,cpv9c ,projekt ,internet ,cpv10c ,cpv11c ,cpv12c ,cpv13c ,cpv14c ,cpv15c ,cpv16c ,cpv17c ,cpv18c ,cpv19c ,cpv20c ,cpv21c ,nr_miesz ,captcha_sec_code ,query ) Values ('"+biuletyn + "','" + pozycja + "','" + data_publikacji + "','" + nazwa + "','" + ulica + "','" + nr_domu + "','" + miejscowosc + "','" + kod_poczt + "','" + wojewodztwo + "','" + tel + "','" + fax + "','" + regon + "','" + e_mail + "','" + ogloszenie + "','" + nrbiuletynu + "','" + nrpozycji + "','" + datawydaniabiuletynu + "','" +  czy_obowiazkowa + "','" + czy_biul_pub + "','" + biul_pub_rok + "','" + biul_pub_poz + "','" + czy_zmiana_ogl + "','" + dotyczy + "','" + rodzaj_zam + "','" + nazwa_zamowienia + "','" + rodz_zam + "','" + przedmiot_zam + "','" + cpv1c + "','" + cpv2c + "','" + kod_trybu + "','" + zamowienie_ue + "','" + zal_pprawna_hid + "','" + zal_pprawna + "','" + zal_uzasadnienie + "','" + rodzaj_zam_inny + "','" + cpv3c + "','" + cpv4c + "','" + cpv5c + "','" + cpv6c + "','" + cpv7c + "','" + cpv8c + "','" + cpv9c + "','" + projekt + "','" + internet + "','" + cpv10c + "','" + cpv11c + "','" + cpv12c + "','" + cpv13c + "','" + cpv14c + "','" + cpv15c + "','" + cpv16c + "','" + cpv17c + "','" + cpv18c + "','" + cpv19c + "','" + cpv20c + "','" + cpv21c + "','" + nr_miesz + "','" + captcha_sec_code + "','" + query + "')";
                            string myInsertQuery = "INSERT INTO large_tabele_"+numerOglo+" (" + rowNames + ") Values (" + rowValues + "')";
                            //ok string myInsertQuery = "INSERT INTO tabele_403 (biuletyn , pozycja , data_publikacji , ulica , nr_domu , miejscowosc , kod_poczt , wojewodztwo , tel , fax , internet , regon , e_mail , ogloszenie  , nrbiuletynu , nrpozycji , datawydaniabiuletynu , czy_obowiazkowa , czy_biul_pub , biul_pub_rok , biul_pub_poz , czy_zmiana_ogl , dotyczy , rodzaj_zam , rodzaj_zam_inny , nazwa_zamowienia , rodz_zam , przedmiot_zam , cpv1c , cpv2c , cpv3c ,cpv4c ,cpv5c ,cpv6c ,cpv7c ,cpv8c ,cpv9c ,cpv10c ,cpv11c ,cpv12c ,cpv13c ,cpv14c ,cpv15c ,cpv16c ,cpv17c ,cpv18c ,cpv19c ,cpv20c ,cpv21c ,cpv22c ,cpv23c ,cpv24c ,cpv25c ,cpv26c ,cpv27c ,cpv28c , cpv29c , kod_trybu , zamowienie_ue , projekt ,wartosc, zal_pprawna_hid , zal_pprawna , zal_uzasadnienie )  Values('" + biuletyn + "','" + poz + "','" + data_publikacji + "','" + ulica + "','" + nr_domu + "','" + miejscowosc + "','" + kod_pocztowy + "','" + wojewodztwo + "','" + tel + "','" + fax + "','" + internet + "','" + regon + "','" + e_mail + "','" + ogloszenie + "','" + nrbiuletynu + "','" + nrpozycji + "','" + datawydaniabiuletynu + "','" + czy_obowiazkowa + "','" + czy_biul_pub + "','" + biul_pub_rok + "','" + biul_pub_poz + "','" + czy_zmiana_ogl + "','" + dotyczy + "','" + rodzaj_zam + "','" + rodzaj_zam_inny + "','" + nazwa_zamowienia + "','" + rodz_zam + "','" + przedmiot_zam + "','" + cpv1c + "','" + cpv2c + "','" + cpv3c + "','" + cpv4c + "','" + cpv5c + "','" + cpv6c + "','" + cpv7c + "','" + cpv8c + "','" + cpv9c + "','" + cpv10c + "','" + cpv11c + "','" + cpv12c + "','" + cpv13c + "','" + cpv14c + "','" + cpv15c + "','" + cpv16c + "','" + cpv17c + "','" + cpv18c + "','" + cpv19c + "','" + cpv20c + "','" + cpv21c + "','" + cpv22c + "','" + cpv23c + "','" + cpv24c + "','" + cpv25c + "','" + cpv26c + "','" + cpv27c + "','" + cpv28c + "','" + cpv29c + "','" + kod_trybu + "','" + zamowienie_ue + "','" + projekt + "','" + wartosc + "','" + zal_pprawna_hid + "','" + zal_pprawna + "','" + zal_uzasadnienie + "')";
                            //string myInsertQuery = "INSERT INTO tabele_403 (biuletyn , pozycja , data_publikacji , ulica , nr_domu )  Values('" + biuletyn + "','" + poz + "','" + data_publikacji + "','" + ulica + "','" + nr_domu + "')";
                            //string myInsertQuery = "CREATE TABLE small_tabele_403 LIKE production.recipes; 
                            //string myInsertQuery = INSERT recipes_new SELECT * FROM production.recipes;)";
                            //tbd trzeba dodać wszystkie 400 do 403  na podstawie xmla.
                            MySqlCommand myCommand = new MySqlCommand(myInsertQuery);
                            myCommand.Connection = myConnection;

                            

                            try
                            {
                                myConnection.Open();
                            }
                            catch (System.Exception ex)
                            {
                                throw new System.Exception(ex.Message, ex.InnerException);
                            }
                            myCommand.ExecuteNonQuery();
                            myCommand.Connection.Close();

                        }
                        catch (Exception e)
                        {

                            logBox("An exception of type " + e.GetType() + " was encountered while inserting the data.");
                            //nie udalo sie dodać do bazy.                        
                        }

                    }

                }

            }

            
        }

        public void downloadFiles()
        {
            logBox("Starting downloading files");
            connectFTP();
            connectDatabase();
            logBox("Files ready at directory");
        }

        
        private void button1_Click(object sender, EventArgs e)
        {
          
            downloadFiles();
            logBox("files downloaded and extracted");

        }

        private void button2_Click_1(object sender, EventArgs e)
        {
            
            updateTables();
        
        }

        private void button4_Click(object sender, EventArgs e)
        {

            string MyConString = "Server=sql.naprzetarg.pl;" + "Database=aniaikarol;" + "Uid=aniaikarol;" + "Pwd=!NaPrzetarg;";
            
            MySqlConnection myConnection = new MySqlConnection(MyConString);
            
            
            try
            {
                myConnection.Open();
                MySqlCommand cmd = myConnection.CreateCommand();
                cmd.CommandText = "Select * FROM tabele_403 where cpv1c = 302360002 ";
                MySqlDataAdapter adap = new MySqlDataAdapter(cmd);
                DataSet ds = new DataSet();
                adap.Fill(ds);
                //dataGridView1.DataSource = ds.Tables[0].DefaultView;

            }
            catch (System.Exception ex)
            {
                throw new System.Exception(ex.Message, ex.InnerException);
            }
            //finally
            //    if (myConnection.State == my .Open)
            //    {
            //    myConnection.clone()
            //    }

        }

        private void button5_Click(object sender, EventArgs e)
        {
            
            logBox("Connecting DB");
                            logBox("Creating command");

                            string MyConString = "Server=sql.naprzetarg.pl;" +"Database=aniaikarol;" + "Uid=aniaikarol;" + "Pwd=!NaPrzetarg;";
                            MySqlConnection connection = new MySqlConnection(MyConString);
                            MySqlCommand command = connection.CreateCommand();
                            MySqlConnection myConnection = new MySqlConnection(MyConString);
                            string myInsertQuery = "CREATE TABLE IF NOT EXISTS large_tabele_403( id INT NOT NULL AUTO_INCREMENT,PRIMARY KEY(id), biuletyn VARCHAR(1),pozycja VARCHAR(6),data_publikacji DATE ,nazwa VARCHAR(237),ulica VARCHAR(50),nr_domu VARCHAR(6),miejscowosc VARCHAR(35),kod_poczt VARCHAR(6),wojewodztwo VARCHAR(19),tel VARCHAR(30),fax VARCHAR(30),regon VARCHAR(14),e_mail VARCHAR(99),ogloszenie VARCHAR(6), nrbiuletynu VARCHAR(1),nrpozycji VARCHAR(6),datawydaniabiuletynu DATE, czy_obowiazkowa VARCHAR(1),czy_biul_pub VARCHAR(1),biul_pub_rok VARCHAR(4),biul_pub_poz VARCHAR(10),czy_zmiana_ogl VARCHAR(1),dotyczy VARCHAR(1),rodzaj_zam VARCHAR(95),nazwa_zamowienia VARCHAR(590),rodz_zam VARCHAR(1),przedmiot_zam VARCHAR(28991),cpv1c VARCHAR(9),cpv2c VARCHAR(9),kod_trybu VARCHAR(2),zamowienie_ue VARCHAR(1),zal_pprawna_hid VARCHAR(6),zal_pprawna VARCHAR(85),zal_uzasadnienie VARCHAR(28347),rodzaj_zam_inny VARCHAR(89),cpv3c VARCHAR(9),cpv4c VARCHAR(9),cpv5c VARCHAR(9),cpv6c VARCHAR(9),cpv7c VARCHAR(9),cpv8c VARCHAR(9),cpv9c VARCHAR(9),projekt VARCHAR(496),internet VARCHAR(94),cpv10c VARCHAR(9),cpv11c VARCHAR(9),cpv12c VARCHAR(9),cpv13c VARCHAR(9),cpv14c VARCHAR(9),cpv15c VARCHAR(9),cpv16c VARCHAR(9),cpv17c VARCHAR(9),cpv18c VARCHAR(9),cpv19c VARCHAR(9),cpv20c VARCHAR(9),cpv21c VARCHAR(9),nr_miesz VARCHAR(5),captcha_sec_code VARCHAR(5),query VARCHAR(496))";
                            MySqlCommand myCommand = new MySqlCommand(myInsertQuery);
                            myCommand.Connection = myConnection;
                            try
                            {
                                myConnection.Open();
                            }
                            catch (System.Exception ex)
                            {
                                throw new System.Exception(ex.Message, ex.InnerException);
                            }
                            myCommand.ExecuteNonQuery();
                            myCommand.Connection.Close();

        }
        private void button6_Click (object sender, EventArgs e)
        {
            

            //Klawisz "Wybierz"
            string numerOglo = comboBox2.Text.ToString();
            logBox("Connecting DB");
            string MyConString = "Server=sql.naprzetarg.pl;" + "Database=aniaikarol;" + "Uid=aniaikarol;" + "Pwd=!NaPrzetarg;"+ "Convert Zero Datetime=True";
            MySqlConnection connection = new MySqlConnection(MyConString);
            MySqlCommand command = connection.CreateCommand();

            MySqlConnection myConnection = new MySqlConnection(MyConString);
            string rodzaj = comboBox3.SelectedValue.ToString();
            string wojew = comboBox4.SelectedValue.ToString();

            //            string myInsertQuery = "SELECT * FROM large_tabele_403 WHERE 1 ORDER BY id DESC Limit "+ comboBox1.Text.ToString();
            string myInsertQuery = "SELECT * FROM large_tabele_" + numerOglo + " WHERE rodz_zam" + rodzaj +" AND wojewodztwo " + wojew +" ORDER BY id DESC Limit " + comboBox1.Text.ToString();
            MySqlCommand myCommand = new MySqlCommand(myInsertQuery);
            myCommand.Connection = myConnection;

            MySqlDataAdapter adap = new MySqlDataAdapter(myCommand);
            DataSet ds = new DataSet();
            adap.Fill(ds);
            //dataGridView1.DataSource = ds.Tables[0].DefaultView;
            // listBox1.Text = ds.Tables["nazwa"].ToString();
            //listBox1.Items.Add(ds.Tables["nazwa"].ToString());
            //listBox1.Items.Add(New ListItem(ds.Tables["nazwa"].ToString()));
            listBox1.DataSource = ds.Tables[0];
           // listBox1.DataTextField =  = "nazwa";
            DataTable dt = ds.Tables[0];
            //listBox1.DisplayMember = dt.Columns[0].ColumnName;
           // listBox1.DisplayMember = dt.Columns["pozycja"].ColumnName;

           // foreach (DataColumn row in dt.Columns["pozycja"].ColumnName)
           // listBox1.Items.Add(row);
            //foreach (DataRow row in dt.Rows)
            //    listBox1.Items.Add(row[0]);

           // label1.Text = listBox1.GetItemText(listBox1.SelectedItem);

            List<string> teams = new List<string>();
            foreach (DataRow dataRow in dt.Rows)
            {
                teams.Add(dataRow["rodz_zam"].ToString() + " " + dataRow["id"].ToString().PadLeft(8,'0') + " " + dataRow["nazwa_zamowienia"].ToString());
            }
            listBox1.DataSource = teams;


            try
            {
                myConnection.Open();
            }
            catch (System.Exception ex)
            {
                throw new System.Exception(ex.Message, ex.InnerException);
            }
            myCommand.ExecuteNonQuery();
            myCommand.Connection.Close();
            

        }
        private void listBox1_SelectionChanged(object sender, EventArgs e)
        {
         
            string Text = listBox1.GetItemText(listBox1.SelectedItem);
            string rodzZam=comboBox3.SelectedValue.ToString();
            DataSet ds = returnDS(listBox1.GetItemText(listBox1.SelectedItem).Substring(2, 8));

                DataTable dt = ds.Tables[0];
            
                textBox2.Text = dt.Rows[0]["przedmiot_zam"].ToString();
                if (dt.Columns.Contains("cena_cz_0")) textBox3.Text = dt.Rows[0]["cena_cz_0"].ToString();
                if (dt.Columns.Contains("wartosc_cz_0")) textBox4.Text = dt.Rows[0]["wartosc_cz_0"].ToString();
                if (dt.Columns.Contains("cena_min_cz_0")) textBox5.Text = dt.Rows[0]["cena_min_cz_0"].ToString();
                if (dt.Columns.Contains("cena_max_cz_0")) textBox6.Text = dt.Rows[0]["cena_max_cz_0"].ToString();
                if (dt.Columns.Contains("liczba_ofert_cz_0")) textBox7.Text = dt.Rows[0]["liczba_ofert_cz_0"].ToString();
                if (dt.Columns.Contains("wykonawcy_cz_0")) textBox8.Text = dt.Rows[0]["wykonawcy_cz_0"].ToString();
                if (dt.Columns.Contains("nazwa_zamowienia")) textBox9.Text = dt.Rows[0]["nazwa_zamowienia"].ToString();
                if (dt.Columns.Contains("rodzaj_zam")) textBox10.Text = dt.Rows[0]["rodzaj_zam"].ToString();
                if (dt.Columns.Contains("miejscowosc")) textBox11.Text = dt.Rows[0]["miejscowosc"].ToString();
                if (dt.Columns.Contains("wojewodztwo")) textBox12.Text = dt.Rows[0]["wojewodztwo"].ToString();
                if (dt.Columns.Contains("internet")) textBox13.Text = dt.Rows[0]["internet"].ToString();

           
        }

        public DataSet returnDS(String pozycja)
        {
            pozycja = pozycja.TrimStart('0');

            logBox("Connecting DB");
            string MyConString = "Server=sql.naprzetarg.pl;" + "Database=aniaikarol;" + "Uid=aniaikarol;" + "Pwd=!NaPrzetarg;" + "Convert Zero Datetime=True";
            MySqlConnection connection = new MySqlConnection(MyConString);
            MySqlCommand command = connection.CreateCommand();

            MySqlConnection myConnection = new MySqlConnection(MyConString);
            //  listBox1.Items.Add();
            string myInsertQuery = "SELECT * FROM large_tabele_" + comboBox2.Text.ToString() + " WHERE id=" + pozycja + " Limit 1";
            MySqlCommand myCommand = new MySqlCommand(myInsertQuery);
            myCommand.Connection = myConnection;

            MySqlDataAdapter adap = new MySqlDataAdapter(myCommand);
            DataSet ds = new DataSet();
            adap.Fill(ds);
            return ds;
        //    DataTable dt = ds.Tables[0];
        //    string nter= dt.Columns["przedmiot_zam"].ColumnName;
           
          //  return dt.Rows[0]["przedmiot_zam"].ToString();

        }

        private void Form1_Load(object sender, EventArgs e)
        {

        }

        private void textBox10_TextChanged(object sender, EventArgs e)
        {

        }

        public void alterTableRename(string table, string name) 
        {
            logBox("Renamed "+ table+ " with "+ name);
            string MyConString = "Server=sql.naprzetarg.pl;" +"Database=aniaikarol;" + "Uid=aniaikarol;" + "Pwd=!NaPrzetarg;";
            MySqlConnection connection = new MySqlConnection(MyConString);
            MySqlCommand command = connection.CreateCommand();
            MySqlConnection myConnection = new MySqlConnection(MyConString);
            string myInsertQuery = "ALTER TABLE large_tabele_"+ table +" RENAME "+ name;
            MySqlCommand myCommand = new MySqlCommand(myInsertQuery);
            myCommand.Connection = myConnection;
            try
            {
                myConnection.Open();
            }
            catch (System.Exception ex)
            {
                throw new System.Exception(ex.Message, ex.InnerException);
            }
            myCommand.ExecuteNonQuery();
            myCommand.Connection.Close();
        }

        public void alterTableModifyColumn(string table, string column, string dataType, int size)
        {
            logBox("Modifing table " + table + " with " + column + " " + dataType + "( " + size + ")");
            string MyConString = "Server=sql.naprzetarg.pl;" + "Database=aniaikarol;" + "Uid=aniaikarol;" + "Pwd=!NaPrzetarg;";
            MySqlConnection connection = new MySqlConnection(MyConString);
            MySqlCommand command = connection.CreateCommand();
            MySqlConnection myConnection = new MySqlConnection(MyConString);
            string myInsertQuery = "ALTER TABLE large_tabele_" + table + " MODIFY " + column +" "+dataType +"("+size+")";
            MySqlCommand myCommand = new MySqlCommand(myInsertQuery);
            myCommand.Connection = myConnection;
            try
            {
                myConnection.Open();
            }
            catch (System.Exception ex)
            {
                throw new System.Exception(ex.Message, ex.InnerException);
            }
            try

            {

                myCommand.ExecuteNonQuery();
            }
            catch (System.Exception ex)

            {
                //throw new System.Exception(ex.Message, ex.InnerException)
                logBox(ex.Message);

            }
            myCommand.Connection.Close();
        }

        public void alterTableAddColumn(string table, string column, string dataType, int size)
        {
            logBox("Adding table " + table + " with " + column + " " + dataType + "( " + size + ")");
            string MyConString = "Server=sql.naprzetarg.pl;" +"Database=aniaikarol;" + "Uid=aniaikarol;" + "Pwd=!NaPrzetarg;";
            MySqlConnection connection = new MySqlConnection(MyConString);
            MySqlCommand command = connection.CreateCommand();
            MySqlConnection myConnection = new MySqlConnection(MyConString);
            string myInsertQuery = "ALTER TABLE large_tabele_" + table + " ADD " + column + " " + dataType + "(" + size + ")";
            MySqlCommand myCommand = new MySqlCommand(myInsertQuery);
            myCommand.Connection = myConnection;
            try
            {
                myConnection.Open();
            }
            catch (System.Exception ex)
            {
                throw new System.Exception(ex.Message, ex.InnerException);
            }
            try
            {
                myCommand.ExecuteNonQuery();
            }
            catch (System.Exception ex)
            {
              //  throw new System.Exception(ex.Message, ex.InnerException);
                logBox(ex.Message);
            }
            myCommand.Connection.Close();
        }
        public void alterTableDropColumn(string table, string column )
        {
            logBox("Droping from table " + table + " column " + column );
            string MyConString = "Server=sql.naprzetarg.pl;" +"Database=aniaikarol;" + "Uid=aniaikarol;" + "Pwd=!NaPrzetarg;";
            MySqlConnection connection = new MySqlConnection(MyConString);
            MySqlCommand command = connection.CreateCommand();
            MySqlConnection myConnection = new MySqlConnection(MyConString);
            string myInsertQuery = "ALTER TABLE large_tabele_" + table + " DROP " + column;
            MySqlCommand myCommand = new MySqlCommand(myInsertQuery);
            myCommand.Connection = myConnection;
            try
            {
                myConnection.Open();
            }
            catch (System.Exception ex)
            {
                throw new System.Exception(ex.Message, ex.InnerException);
            }
            myCommand.ExecuteNonQuery();
            myCommand.Connection.Close();
        }
        private void button8_Click(object sender, EventArgs e)
        {
            string ZPtypex = "403";
            int startlen = 10;
            int len = 12;
            alterTableAddColumn(ZPtypex, "nowalll", "VARCHAR", startlen);
            alterTableModifyColumn(ZPtypex, "nowalll", "VARCHAR", len);
            alterTableDropColumn(ZPtypex, "nowalll");

        }

        private void comboBox2_SelectedIndexChanged(object sender, EventArgs e)
        {

        }

        private void comboBox3_SelectedIndexChanged(object sender, EventArgs e)
        {

        }
       
// Get combobox selection (in handler)
//string value = ((KeyValuePair<string, string>)comboBox1.SelectedItem).Value; 
    }
}
