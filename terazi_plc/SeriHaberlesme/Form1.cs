using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;                    // Buradaki kütüphaneleri bilgisayarımıza ekliyoruz...
using System.Text;
using System.Windows.Forms;
using System.IO.Ports;
using Microsoft.VisualBasic;
using WindowsFormsApplication2;
using SCADA;
using System.Threading;
using System.Data.SqlClient;
using Microsoft.VisualBasic.PowerPacks;

namespace SeriHaberlesme
{
    public partial class Form1 : Form
    {
        SqlDataAdapter da;
        DataSet ds;

        public Form1()
        {
            InitializeComponent();
        }

        public SqlConnection baglanti()
        {
            SqlConnection bag = new SqlConnection("Data Source=mzroboteam.database.windows.net;Initial Catalog=mzroboteam;Persist Security Info=True;User ID=mzroboteam;Password=mzorlu+2018");
            bag.Open();
            return bag;
        }
        int u,n,m;
        int ireturncode;
        int veri1;
        SCADA.Form1 scadafrm = new SCADA.Form1();

        private void COMPortlariListele()
        {
            string[] myPort;
            // Burada ise bilgisayara bağlanan portları okuyup comboBoxa listeliyoruz...
            myPort = System.IO.Ports.SerialPort.GetPortNames();
            comboBoxCOMPorts.Items.AddRange(myPort);

        }

        private void Form1_Load(object sender, EventArgs e)
        {
            
            // TODO: This line of code loads data into the 'mzteraziDataSet.urun' table. You can move, or remove it, as needed.
          //  this.urunTableAdapter.Fill(this.mzteraziDataSet.urun);
            COMPortlariListele();
            comboBoxSerialPortBaudRate.SelectedIndex = 0;
           timergirisoku1.Enabled = true;
            timergirisoku2.Enabled = true;
            this.urunTableAdapter1.Fill(this.mzroboteamDataSet.urun);
        }

        delegate void GelenVerileriGuncelleCallback(string veri);

        private void GelenVerileriGuncelle(string veri)
        {
            int k;
            int IRet ;
            label7.Text=veri.ToString();
            if (u == 0)
            {
                    if ((veri.Substring(2, 1) == "0") || (veri.Substring(3, 1) == "0") && (veri.Substring(4, 1) != "0") && (veri.Length <= 15))
                    {
                        veri = veri.Substring(4, 2);                                // Burada veri olarak tanımlanan string ifadenin 4'üncü karakterinden 6'ncı karakteri de dahil 
                        label2.Text = " ";
                        label5.Text = "";
                        string q = veri.Substring(1, 1);                                 // olmak üzere kesilip tekrardan veri değişkenine atıyoruz ve label de gösteriyoruz... 
                        string w = veri.Substring(0, 1);
                        if ((w == "9" || w == "8" || w == "7" || w == "6" || w == "5" || w == "4" || w == "3" || w == "2" || w == "1" || w == "0") && (q == "9" || q == "8" || q == "7" || q == "6" || q == "5" || q == "4" || q == "2" || q == "3" || q == "1" || q == "0"))
                        {
                            label5.Text = veri + " g";
                            terazi_ekran.Text=veri;
                            veri1 = Convert.ToInt16(veri);
                            veri = "";

                            if ((veri1 >= 55)&&(veri1 != 00))
                            {
                                k = 3;
                                IRet = axActProgType1.WriteDeviceBlock("D100", 1, ref(k));
                                yazdir.Enabled = true;
                                sag1.Visible = true;
                                sag2.Visible = true;
                                sag3.Visible = true;
                                sol1.Visible = false;
                                sol2.Visible = false;
                                sol3.Visible = false;
                                timerSerialPort.Enabled=false;  
                            }
                            else if ((veri1 > 0) && (veri1 <= 55)&&(veri1 != 00))
                            {
                                k = 1;
                                IRet = axActProgType1.WriteDeviceBlock("D100", 1, ref(k));
                                yazdir.Enabled = true;
                                sol1.Visible = true;
                                sol2.Visible = true;
                                sol3.Visible = true;
                                sag1.Visible = false;
                                sag2.Visible = false;
                                sag3.Visible = false;
                                timerSerialPort.Enabled = false;  
                            }

                            
                            
                        }
                        else
                        {
                            /*Thread.Sleep(2000);
                            label2.Text = " BEKLEYİN...  ";
                            label5.Text = "";
                            label6.Text = "YÜKSEK AĞIRLIK";
                            Thread.Sleep(2000);*/
                        }
                    }
            }
            else if (u==1)
            {
                label2.Text = " BEKLEYİN...  ";
            }
        }
        private void urunler()
        {
            SqlConnection bag = baglanti();
            bag = new SqlConnection("Data Source=mzroboteam.database.windows.net;Initial Catalog=mzroboteam;Persist Security Info=True;User ID=mzroboteam;Password=mzorlu+2018");
            da = new SqlDataAdapter("Select *from dbo.urun order by tarih desc",bag);
            ds = new DataSet();
            bag.Open();
            da.Fill(ds,"urun");
            dataGridView1.DataSource=ds.Tables["urun"];
        }
        private void buttonSerialPortBaglanti_Click(object sender, EventArgs e)
        {
            long port_ac;
            axActProgType1.ActPortNumber = 3;//**
            port_ac = axActProgType1.Open();

            if (comboBoxCOMPorts.SelectedIndex < 0)                    // comboBoxCOMPorts da bir değer seçilmemiş ise yani com seçilmedi ise
            {                                                          // uyarı mesajı olarak "COM port bulunamadi" hatasını gönderiyoruz...
                MessageBox.Show("COM port bulunamadi",Name ,MessageBoxButtons.OK ,MessageBoxIcon .Error );
                return;
            }

            if (comboBoxSerialPortBaudRate.SelectedIndex < 0)
            {                                                         // comboBoxSerialPortBaudRate de bir değer seçilmedi ise yani haberleşme 
                MessageBox.Show("Bağlantı hızı seçiniz");             // hızı seçilmedi ise "Bağlantı hızı seçiniz" diye bir hata mesajı gönderiyoruz...
                return;
            }

            try
            {
                if (serialPort1.IsOpen == false)
                {
                    serialPort1.PortName = comboBoxCOMPorts.SelectedItem.ToString(); // seri porta seçilen portu aktarıyoruz...
                    serialPort1.BaudRate = Convert.ToInt32(comboBoxSerialPortBaudRate.Text); // haberleşme hızını int ye çeviriyoruz...
                    serialPort1.Open();                                                      // ve seriport'u açıyoruz...
                    // TODO: This line of code loads data into the 'mzroboteamDataSet.urun' table. You can move, or remove it, as needed.
                    buttonSerialPortBaglanti.Text = "Teraziye bağlandı...";                          // ve butonumuzun text'ine "Bağlantı kes"
                    timerSerialPort.Enabled = true;                                                      // yazıyoruz... ve timerimizi açıyoruz...
                    button1.Enabled = true;
                    button2.Enabled =true ;
                }
                else
                {
                    timerSerialPort.Enabled = false;                                         // timeri kapatıyoruz , seriportu kapatıyoruz
                    serialPort1.Close();                                                     // ve butonumuzun text'ine "Bağlantı aç "
                    buttonSerialPortBaglanti.Text = "Teraziye bağlan";                           // yazıyoruz....
                    label5.Text =" ";
                }
            }
            catch
            {                                                                                // ve eğer teknik bir hata veya seri port ile 
                MessageBox.Show("TERAZİYE BAĞLANILAMADI!");                                      // ilgili bir hata olur ise kendi hata 
            }                                                                                // mesajımızı yazdık...
        }

        private void timerSerialPort_Tick(object sender, EventArgs e)
        {
            if (serialPort1.BytesToRead > 0)
            {
                GelenVerileriGuncelle(serialPort1.ReadExisting());

            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            int ireturncode;
            try
            {
                ireturncode = axActProgType1.Close();
                MessageBox.Show("Bağlantı Kesildi",Name ,MessageBoxButtons.OK,MessageBoxIcon.Warning);
                button4.Enabled = false ;
            }
            catch (Exception exeption)
            {
                MessageBox.Show(exeption.Message, Name, MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            try
            {
                axActProgType1.ActCpuType = 0x205;
                axActProgType1.ActUnitType = 0x0F;
                axActProgType1.ActProtocolType = 0x04;
                axActProgType1.ActNetworkNumber = 0;
                axActProgType1.ActStationNumber = 255;
                axActProgType1.ActUnitNumber = 0;
                axActProgType1.ActConnectUnitNumber = 0;
                axActProgType1.ActIONumber = 0;
                axActProgType1.ActPortNumber = 3;
                axActProgType1.ActBaudRate = 9600;
                axActProgType1.ActDataBits = 0x0007;
                axActProgType1.ActParity = 0x0002;
                axActProgType1.ActStopBits = 0x00000;
                axActProgType1.ActControl = 0x08;
                ireturncode = axActProgType1.Open();
                if (ireturncode == 0)
                {
                    MessageBox.Show("Bağlantı Kuruldu",Name ,MessageBoxButtons.OK  ,MessageBoxIcon.Information);
                    button4.Enabled = true;
                }

            }
            catch (Exception exeption)
            {
                MessageBox.Show(exeption.Message , Name ,MessageBoxButtons.OK  ,MessageBoxIcon.Error);
                return ;
                   
            }
           
        }


        private void button4_Click(object sender, EventArgs e)
        {
            int st, s = 255, o = 0;

            st = axActProgType1.WriteDeviceRandom("M35", 1, ref (s));
            st = axActProgType1.WriteDeviceRandom("M35", 1, ref (o));
        }

        private void D0_CheckedChanged(object sender, EventArgs e)
        {
            int veri = 255, veri_yaz = 0, J = 0;
            if (D0.Checked == true)
            {
                l37.BackColor = Color.Red;
                veri_yaz = axActProgType1.WriteDeviceRandom("M60", 1, ref(veri));
            }
            else
            {
                l37.BackColor = Color.White;
                veri_yaz = axActProgType1.WriteDeviceRandom("M60", 1, ref(J));
            }
        }

        private void D2_CheckedChanged(object sender, EventArgs e)
        {
            int veri = 255, veri_yaz = 0, J = 0;
            if (D1.Checked == true)
            {
                l38.BackColor = Color.Red;
                veri_yaz = axActProgType1.WriteDeviceRandom("M65", 1, ref(veri));
            }
            else
            {
                l38.BackColor = Color.White;
                veri_yaz = axActProgType1.WriteDeviceRandom("M65", 1, ref(J));
            }
        }

        private void D3_CheckedChanged(object sender, EventArgs e)
        {
            int veri = 255, veri_yaz = 0, J = 0;
            if (D2.Checked == true)
            {
                l39.BackColor = Color.Red;
                veri_yaz = axActProgType1.WriteDeviceRandom("M70", 1, ref(veri));
            }
            else
            {
                l39.BackColor = Color.White;
                veri_yaz = axActProgType1.WriteDeviceRandom("M70", 1, ref(J));
            }
        }

        private void D3_CheckedChanged_1(object sender, EventArgs e)
        {
            int veri = 255, veri_yaz = 0, J = 0;
            if (D3.Checked == true)
            {
                l40.BackColor = Color.Red;
                veri_yaz = axActProgType1.WriteDeviceRandom("M75", 1, ref(veri));
            }
            else
            {
                l40.BackColor = Color.White;
                veri_yaz = axActProgType1.WriteDeviceRandom("M75", 1, ref(J));
            }
        }

        private void D4_CheckedChanged(object sender, EventArgs e)
        {
            int veri = 255, veri_yaz = 0, J = 0;
            if (D4.Checked == true)
            {
                l41.BackColor = Color.Red;
                veri_yaz = axActProgType1.WriteDeviceRandom("M80", 1, ref(veri));
            }
            else
            {
                l41.BackColor = Color.White;
                veri_yaz = axActProgType1.WriteDeviceRandom("M80", 1, ref(J));
            }
        }

        private void D5_CheckedChanged(object sender, EventArgs e)
        {
           
        }

        private void D7_CheckedChanged(object sender, EventArgs e)
        {
           
        }

        private void button3_Click(object sender, EventArgs e)
        {
            WindowsFormsApplication2.Form1 sanalbutonfrm = new WindowsFormsApplication2.Form1();
            sanalbutonfrm.Show();
        }

        private void timer1_Tick_1(object sender, EventArgs e)
        {
            int veri, veri_oku;
            veri_oku = axActProgType1.ReadDeviceRandom("X15", 1, out (veri));
            if (veri == 0)
            {
                u = 0;
                label6.Text = "";
            }
            else
            {
                u = 1;
                label5.Text = "";
                label6.Text = "ACİL STOP BASILI ";
            }
        }

        private void button5_Click(object sender, EventArgs e)
        {
            if (ireturncode == 0)
            {
                    ireturncode = axActProgType1.Close();   
            }
            if (serialPort1.IsOpen == true)
            {
                timerSerialPort.Enabled = false;                                         // timeri kapatıyoruz , seriportu kapatıyoruz
                serialPort1.Close();                                                     // ve butonumuzun text'ine "Bağlantı aç "
                buttonSerialPortBaglanti.Text = "Teraziye bağlan";                           // yazıyoruz....
                label5.Text = "";
            }
            this.Close();
        }

        private void yazdir_Tick(object sender, EventArgs e)
        {
            SqlConnection bag = baglanti();
            SqlCommand komut = new SqlCommand("insert into urun(agirlik,tarih) values (@agirlik,@tarih)", bag);
            komut.Parameters.AddWithValue("@agirlik", (veri1.ToString()));
            komut.Parameters.AddWithValue("@tarih", (DateTime.Now));
            komut.ExecuteNonQuery();
            bag.Close();
            urunler();

            timerSerialPort.Enabled = true;
            yazdir.Enabled = false;
        }

        private void D5_CheckedChanged_1(object sender, EventArgs e)
        {
            int veri = 64, veri_yaz = 0, J = 0;
            if (D5.Checked == true)
            {
                l42.BackColor = Color.Red;
                veri_yaz = axActProgType1.WriteDeviceBlock("Y", 16, ref(veri));
            }
            else
            {
                l42.BackColor = Color.White;
                veri_yaz = axActProgType1.WriteDeviceBlock("Y", 16, ref(J));
            }
        }

        private void D7_CheckedChanged_1(object sender, EventArgs e)
        {
            int veri = 255, veri_yaz = 0, J = 0;
            if (D7.Checked == true)
            {
                l43.BackColor = Color.Red;
                veri_yaz = axActProgType1.WriteDeviceRandom("M85", 1, ref(veri));
            }
            else
            {
                l43.BackColor = Color.White;
                veri_yaz = axActProgType1.WriteDeviceRandom("M85", 1, ref(J));
            }
        }

        private void timergirisoku1_Tick(object sender, EventArgs e)
        {
            int veri_oku;
            int[] veri = new int[4];
            veri_oku = axActProgType1.ReadDeviceRandom("X4", 1, out (veri[0]));
            if (veri[0] == 0)
            {
                label47.BackColor = Color.White;
                hareketsensoru1.BackColor = Color.Red;
                hareketsensoru2.BackColor = Color.Red;
            }
            else
            {
                urun.Left = 107;
                urun.Top = 564;
                label47.BackColor = Color.Red;
                hareketsensoru1.BackColor = Color.Green;
                hareketsensoru2.BackColor = Color.Green;
                n = 1;
                label8.Text = n.ToString();
            }
            veri_oku = axActProgType1.ReadDeviceRandom("X5", 1, out (veri[1]));
            if (veri[1] == 0)
            {
                label49.BackColor = Color.White;
                solsensor1.BackColor = Color.Red;
                solsensor2.BackColor = Color.Red;
            }
            else
            {
                urun.Left = 36;
                urun.Top = 564;
                label49.BackColor = Color.Red;
                solsensor1.BackColor = Color.Green;
                solsensor2.BackColor = Color.Green;
                n = 0;
                sol1.Visible = false;
                sol2.Visible = false;
                sol3.Visible = false;
                label8.Text = n.ToString();
            }
            veri_oku = axActProgType1.ReadDeviceRandom("X6", 1, out (veri[2]));
            if (veri[2] == 0)
            {
                label50.BackColor = Color.White;
                sagsensor1.BackColor = Color.Red;
                sagsensor2.BackColor = Color.Red;
            }
            else
            {
                urun.Left = 35;
                urun.Top = 564;
                label50.BackColor = Color.Red;
                sagsensor1.BackColor = Color.Green;
                sagsensor2.BackColor = Color.Green;
                n = 0;
                label8.Text = n.ToString();
            }
            veri_oku = axActProgType1.ReadDeviceRandom("X7", 1, out (veri[3]));
            if (veri[3] == 0)
            {
                label52.BackColor = Color.White;
            }
            else
            {
                label52.BackColor = Color.Red;
                int j;
                veri_oku = axActProgType1.ReadDeviceRandom("X13", 1, out (j));
                if (j != 0)
                {
                    if (n == 1)
                    {
                        asagisilindircubuk.Height = 32;
                        vakum4.Left = 742;
                        vakum4.Top = 582;
                        vakum5.Left = 729;
                        vakum5.Top = 582;
                        asagisilindirucu.Left = 727;
                        asagisilindirucu.Top = 570;
                        urun.Left = 255;
                        urun.Top = 564;
                        vakum1.Left = 255;
                        vakum1.Top = 556;
                        vakum3.Left = 278;
                        vakum3.Top = 556;
                        vakum2.Left = 267;
                        vakum2.Top = 556;
                        silındirucu.Left = 255;
                        silındirucu.Top = 545;
                        asagisilindirsagcubuk.Height = 30;
                        asagisilindirsolcubuk.Height = 30;
                        direksagust.Height = 28;
                        direksagalt.Height = 8;
                        n = 2;
                        label8.Text = n.ToString();
                    }
                    if (n == 7)
                    {
                        urun1.Top = 591;
                        asagisilindirucu.Left = 727;
                        asagisilindirucu.Top = 570;
                        asagisilindircubuk.Height = 32;
                        vakum4.Top = 582;
                        vakum5.Top = 582;
                        vakum1.Left = 255;
                        vakum1.Top = 556;
                        vakum2.Left = 267;
                        vakum2.Top = 555;
                        vakum3.Left = 278;
                        vakum3.Top = 556;
                        silındirucu.Left = 255;
                        silındirucu.Top = 545;
                        urun.Left = 255;
                        urun.Top = 564;
                        asagisilindirsagcubuk.Height = 30;
                        asagisilindirsolcubuk.Height = 30;
                        direksagust.Height = 28;
                        direksagalt.Height = 8;
                        n = 8;
                        label8.Text = n.ToString();
                    }
                }
            }
        }

        private void timergirisoku2_Tick(object sender, EventArgs e)
        {
            int[] veri = new int[5];
            int veri_oku;
            int j;
            veri_oku = axActProgType1.ReadDeviceRandom("X7", 1, out (j));
            veri_oku = axActProgType1.ReadDeviceRandom("X10", 1, out (veri[0]));
            if (veri[0] == 0)
            {
                label9.BackColor = Color.White;
            }
            else
            {
                label9.BackColor = Color.Red;
                if (m == 1)
                {
                    vakum5.Left = 814;
                    vakum5.Top = 564;
                    vakum4.Left = 801;
                    vakum4.Top = 564;
                    urun1.Top = 572;
                    urun1.Left = 795;
                    asagisilindirucu.Left = 799;
                    asagisilindirucu.Top = 552;
                    asagisilindircubuk.Left = 809;
                    asagisilindir.Left = 799;
                    ilerisilindirustcubuk.BorderWidth = 84;
                    ilerisilindirasagicubuk.Width = 84;
                    label19.Text = m.ToString();
                }
            }
            veri_oku = axActProgType1.ReadDeviceRandom("X11", 1, out (veri[1]));
            if (veri[1] == 0)
            {
                label10.BackColor = Color.White;
            }
            else
            {
                label10.BackColor = Color.Red;
            }
            veri_oku = axActProgType1.ReadDeviceRandom("X12", 1, out (veri[2]));
            if (veri[2] == 0)
            {
                label11.BackColor = Color.White;
            }
            else
            {
                label11.BackColor = Color.Red;
                if (j != 0)
                {
                    if (n == 2)
                    {
                        urun.Left = 255;
                        urun.Top = 545;
                        vakum1.Left = 255;
                        vakum1.Top = 537;
                        vakum2.Left = 267;
                        vakum2.Top = 537;
                        vakum3.Left = 278;
                        vakum3.Top = 537;
                        silındirucu.Left = 255;
                        silındirucu.Top = 527;
                        urun1.Top = 572;
                        vakum4.Top = 564;
                        vakum5.Top = 564;
                        asagisilindircubuk.Left = 737;
                        asagisilindircubuk.Top = 537;
                        asagisilindirucu.Top = 563;
                        asagisilindircubuk.Height = 14;
                        asagisilindirsagcubuk.Height = 12;
                        asagisilindirsolcubuk.Height = 12;
                        direksagust.Height = 12;
                        direksagalt.Height = 36;
                        n = 3;
                        m = 1;
                        label8.Text = n.ToString();
                    }
                    else if (n == 8)
                    {
                        asagisilindircubuk.Height = 14;
                        asagisilindirucu.Top = 563;
                        vakum5.Top = 576;
                        vakum4.Top = 576;
                        vakum1.Left = 255;
                        vakum1.Top = 537;
                        vakum2.Left = 267;
                        vakum2.Top = 537;
                        vakum3.Left = 278;
                        vakum3.Top = 537;
                        silındirucu.Left = 255;
                        silındirucu.Top = 527;
                        asagisilindirsagcubuk.Height = 12;
                        asagisilindirsolcubuk.Height = 12;
                        direksagust.Height = 12;
                        direksagalt.Height = 36;
                        label8.Text = "İŞLEM TAMAMLANDI";
                    }
                    if (m == 2)
                    {
                        urun1.Left = 724;
                        vakum4.Left = 742;
                        vakum5.Left = 729;
                        asagisilindirucu.Left = 727;
                        ilerisilindirustcubuk.Width = 12;
                        ilerisilindirasagicubuk.Width = 12;
                        asagisilindir.Left = 727;
                        asagisilindircubuk.Left = 737;
                    }
                }
                if (n == 4 && veri[0] != 0)
                {
                    vakum1.Left = 255;
                    vakum1.Top = 537;
                    vakum2.Left = 267;
                    vakum2.Top = 537;
                    vakum3.Left = 278;
                    vakum3.Top = 537;
                    silındirucu.Left = 255;
                    silındirucu.Top = 527;
                    asagisilindirsagcubuk.Height = 12;
                    asagisilindirsolcubuk.Height = 12;
                    direksagust.Height = 12;
                    direksagalt.Height = 36;
                    vakum4.Top = 564;
                    vakum5.Top = 564;
                    asagisilindirucu.Top = 552;
                    asagisilindircubuk.Height = 14;
                    n = 5;
                    label8.Text = n.ToString();
                }
                else if (n == 6 && veri[0] != 0)
                {
                    urun1.Top = 572;
                    vakum4.Top = 564;
                    vakum5.Top = 564;
                    m = 2;
                    asagisilindircubuk.Top = 537;
                    asagisilindirucu.Top = 552;
                    asagisilindircubuk.Height = 14;
                    urun.Left = 255;
                    urun.Top = 540;
                    vakum1.Left = 255;
                    vakum1.Top = 537;
                    vakum2.Left = 267;
                    vakum2.Top = 537;
                    vakum3.Left = 278;
                    vakum3.Top = 537;
                    silındirucu.Left = 255;
                    silındirucu.Top = 527;
                    asagisilindirsagcubuk.Height = 12;
                    asagisilindirsolcubuk.Height = 12;
                    direksagust.Height = 12;
                    direksagalt.Height = 36;
                    n = 7;
                    label8.Text = n.ToString();
                }
            }
            veri_oku = axActProgType1.ReadDeviceRandom("X13", 1, out (veri[3]));
            if (veri[3] == 0)
            {
                label29.BackColor = Color.White;
            }
            else
            {
                label29.BackColor = Color.Red;
                if (n == 3 && veri[0] != 0)
                {
                    urun1.Left = 795;
                    urun1.Top = 591;
                    urun.Left = 255;
                    urun.Top = 564;
                    asagisilindircubuk.Height = 32;
                    asagisilindircubuk.Left = 809;
                    asagisilindircubuk.Top = 537;
                    vakum4.Left = 801;
                    vakum4.Top = 583;
                    vakum5.Left = 814;
                    vakum5.Top = 583;
                    asagisilindirucu.Left = 799;
                    asagisilindirucu.Top = 571;
                    vakum1.Left = 255;
                    vakum1.Top = 556;
                    // vakum2.Left = 264;
                    vakum2.Top = 556;
                    vakum3.Left = 278;
                    vakum3.Top = 556;
                    silındirucu.Left = 255;
                    silındirucu.Top = 545;
                    asagisilindirsagcubuk.Height = 30;
                    asagisilindirsolcubuk.Height = 30;
                    direksagust.Height = 28;
                    direksagalt.Height = 8;
                    n = 4;
                    m = 2;
                    label8.Text = n.ToString();
                }
                else if (n == 5 && veri[0] != 0)
                {
                    vakum4.Top = 583;
                    vakum5.Top = 583;
                    asagisilindirucu.Top = 571;
                    asagisilindircubuk.Height = 32;
                    asagisilindircubuk.Left = 809;
                    asagisilindircubuk.Top = 537;
                    vakum1.Left = 255;
                    vakum1.Top = 555;
                    vakum2.Left = 267;
                    vakum2.Top = 556;
                    vakum3.Left = 278;
                    vakum3.Top = 556;
                    silındirucu.Left = 255;
                    silındirucu.Top = 545;
                    asagisilindirsagcubuk.Height = 30;
                    asagisilindirsolcubuk.Height = 30;
                    direksagust.Height = 28;
                    direksagalt.Height = 8;
                    n = 6;
                    label8.Text = n.ToString();
                }
            }
            veri_oku = axActProgType1.ReadDeviceRandom("X17", 1, out (veri[4]));
            if (veri[4] == 0)
            {
                label42.BackColor = Color.White;
            }
            else
            {
                label42.BackColor = Color.Red;
            }
        }
    }
}


