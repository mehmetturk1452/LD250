Imports System
Imports System.Collections.Generic
Imports System.ComponentModel
Imports System.Data
Imports System.Drawing
Imports System.Linq
Imports System.Text
Imports System.Windows.Forms
Imports S7.Net
Imports Microsoft.VisualBasic
Imports Microsoft.VisualBasic.CompilerServices
Imports Mobile.Communication.Client
Imports System.Data.SqlClient
Imports System.Diagnostics
Imports System.IO
Imports System.Net
Imports System.Runtime.CompilerServices
Imports System.Threading
Imports System.Data.DataTable
Imports System.Data.OleDb








Public Class Form1
    Public client1 = New RobotClient("toolkitadmin", IPAddress.Parse("192.168.15.100"), "7171")
    Public sayacadresi(68) As String
    Public Masa(68) As String
    Public Konveyor(68) As String
    Public SonSayac(68) As String
    Public iskon As String
    Public konsayi As Integer

    Dim baglantiASRS As New SqlClient.SqlConnection("Server=192.168.90.240; Database=2Bee_GelalSock;User Id= gelal;Password=gelal123")
    Dim baglantiASRS1 As New SqlClient.SqlConnection("Server=192.168.55.240; Database=OrguPlc_Omron;User Id= sa;Password=Sa123Sa")

    Dim baglantiASRSis As New SqlClient.SqlConnection("Server=192.168.90.240; Database=2Bee_GelalSock;User Id= gelal;Password=gelal123")
    Dim baglantiASRSis1 As New SqlClient.SqlConnection("Server=192.168.55.240; Database=OrguPlc_Omron;User Id= sa;Password=Sa123Sa")


    Dim yazmaBaglanti As New SqlClient.SqlConnection("Server=192.168.55.240; Database=OrguPlc_Omron;User Id= sa;Password=Sa123Sa")
    Dim yazmaBaglantisayi As New SqlClient.SqlConnection("Server=192.168.55.240; Database=OrguPlc_Omron;User Id= sa;Password=Sa123Sa")
    Dim okumaBaglantisayi As New SqlClient.SqlConnection("Server=192.168.55.240; Database=OrguPlc_Omron;User Id= sa;Password=Sa123Sa")

    Dim Guncellemebaglanti As New SqlClient.SqlConnection("Server=192.168.90.240; Database=2Bee_GelalSock;User Id= gelal;Password=gelal123")
    Dim Guncellemebaglanti1 As New SqlClient.SqlConnection("Server=192.168.55.240; Database=OrguPlc_Omron;User Id= sa;Password=Sa123Sa")

    Public plc As New Plc(CpuType.S71500, "192.168.15.60", 0, 1)


    Dim ASRSkomut As New SqlClient.SqlCommand
    Dim ASRSkomutis As New SqlClient.SqlCommand
    Dim YazmaKomut As New SqlClient.SqlCommand
    Dim YazmaKomutsayi As New SqlClient.SqlCommand
    Dim OkumaKomutSayi As New SqlClient.SqlCommand
    Dim GuncellemeKomut As New SqlClient.SqlCommand


    Dim kapiisvar(7) As Boolean
    Dim data As New DataSet

    Public Sub goster()
        baglantiASRS.Open()
        data.Clear()
        Dim adaptor As New SqlClient.SqlDataAdapter("select * from [dbo].[IC_KasaHazir]", baglantiASRS)
        adaptor.Fill(data, "sanal")
        DataGridView1.DataSource = data.Tables("sanal")
        baglantiASRS.Close()
    End Sub
    Public Sub is_gonder()
        ASRSkomut.CommandText = "Select kh_isemrino,kh_kapiid,kh_masaid, count(kh_kat) as kacsira 
FROM [dbo].[IC_KasaHazir] 
where kh_kat=1 and kh_hazir=1 and kh_masaid<>0 and kh_gorevvar<>'True'  
group by kh_isemrino,kh_kapiid,kh_masaid having count(kh_kat)>0"
        baglantiASRS.Open()


        ASRSkomut.Connection = baglantiASRS
        ASRSkomut.CommandType = CommandType.Text
        Dim okunan As SqlDataReader
        okunan = ASRSkomut.ExecuteReader()
        While (okunan.Read())

            If okunan("kh_kapiid") IsNot "" Then
                Dim i As Integer

                konsayi = 1
                i = 1
                While i < sayacadresi.Count
                    If okunan("kh_masaid") = Convert.ToInt32(Masa(i).Replace("Formhane", "")) And SonSayac(i) < 7 Then

                        Exit While

                    Else
                        If okunan("kh_masaid") = Convert.ToInt32(Masa(i).Replace("Formhane", "")) And SonSayac(i) > 7 Then
                            konsayi += 1

                        End If

                        'TextBox3.Text = SonSayac(i)
                        'TextBox2.Text = "Formhane" & okunan("kh_masaid")
                    End If
                    i += 1
                End While


                If 0 < konsayi And konsayi < 4 Then

                    Dim ex As System.Exception
                    Try
                        client1.sendcommand("queueMulti 2 2 " & "ASRSKapi" & okunan("kh_kapiid") & "-" & okunan("kacsira") & " pickup 10 LD250_Formhane" & okunan("kh_masaid") & "-" & konsayi & " dropoff 20")
                        TextBox1.Text = "queueMulti 2 2 " & "ASRSKapi" & okunan("kh_kapiid") & "-" & okunan("kacsira") & " pickup 10 LD250_Formhane" & okunan("kh_masaid") & "-" & iskon & " dropoff 20"

                        baglantiASRSis.Open()
                        ASRSkomutis.Connection = baglantiASRSis
                        ASRSkomutis.CommandType = CommandType.Text
                        ASRSkomutis.CommandText = "Select * from [dbo].[IC_KasaHazir] where kh_kapiid='" & okunan("kh_kapiid") & "' and kh_isemrino='" & okunan("kh_isemrino") & "'"
                        Dim okunanis As SqlDataReader
                        okunanis = ASRSkomutis.ExecuteReader()
                        yazmaBaglanti.Open()
                        YazmaKomut.Connection = yazmaBaglanti
                        Guncellemebaglanti.Open()
                        GuncellemeKomut.Connection = Guncellemebaglanti

                        While (okunanis.Read())
                            Dim istasyon As String
                            If okunanis("kh_kapiid").ToString.Length = 1 Then
                                istasyon = "0" & okunanis("kh_kapiid").ToString
                            Else
                                istasyon = okunanis("kh_kapiid").ToString
                            End If
                            GuncellemeKomut.CommandText = "UPDATE [dbo].[IC_KasaHazir] SET [kh_gorevvar] = 'True' WHERE kh_kasaid='" & okunanis("kh_kasaid") & "'"
                            GuncellemeKomut.ExecuteNonQuery()




                            YazmaKomut.CommandText = "INSERT INTO [dbo].[YMASRS_HazirKasaTablosu]
           ([IsEmriNumarasi]
           ,[KasaID]
           ,[ForhaneIsIstasyonu]
           ,[Tarih])
     VALUES
           ('" & okunanis("kh_isemrino") & "'
           ,'" & okunanis("kh_kasaid") & "'
           ,'Formhane" & istasyon & "'
           ,Getdate())"
                            YazmaKomut.ExecuteNonQuery()
                        End While
                        Guncellemebaglanti.Close()
                        baglantiASRSis.Close()

                        Label1.Text = "Bağlantı Sağladı"
                        CheckBox200.Checked = True

                    Catch ex
                        Label1.Text = "Bağlantı Düştü"
                        CheckBox200.Checked = False
                    End Try
                End If
                Exit While
                End If


        End While

        baglantiASRS.Close()
        yazmaBaglanti.Close()
    End Sub

    Private Sub Form1_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        Call degiskenatama()
        Call goster()



    End Sub

    Private Sub timer1_Tick(sender As Object, e As EventArgs) Handles timer1.Tick
        Call goster()
        Call is_gonder()

    End Sub

    Private Sub Btnbaglan_Click(sender As Object, e As EventArgs) Handles Btnbaglan.Click
        Try

            client1.Password = "1234"
            client1.Connect()

            CheckBox200.Checked = True
            Label1.Text = "Bağlantı Sağlandı"
            PLC_okumaTimer.Start()
            timer1.Start()


        Catch exception1 As Exception

            MsgBox("Robotlara bağlantı sağlanamadı. Lütfen internet ayarlarını kontrol ettikten sonra tekrar deneyiniz.")
            Exit Sub

        End Try

    End Sub

    Private Sub btnStop_Click(sender As Object, e As EventArgs) Handles btnStop.Click
        Close()

    End Sub

    Private Sub PLC_okumaTimer_Tick(sender As Object, e As EventArgs) Handles PLC_okumaTimer.Tick
        Dim i As Integer
        i = 1
        While i < sayacadresi.Count

            plc.Open()


            yazmaBaglantisayi.Open()
            YazmaKomutsayi.Connection = yazmaBaglantisayi
            YazmaKomutsayi.CommandText = "UPDATE [dbo].[YMASRS_FormhaneIstasyonKasaSayıları]
   SET [KasaSayisi] = '" & plc.Read(sayacadresi(i)) & "'
 WHERE [MakineNumarasi]='" & Masa(i) & "' and [Konveyor]='" & Konveyor(i) & "'"
            SonSayac(i) = plc.Read(sayacadresi(i))
            YazmaKomutsayi.ExecuteNonQuery()
            yazmaBaglantisayi.Close()
            plc.Close()


            i += 1
        End While


    End Sub

    Public Sub degiskenatama()
        OkumaKomutSayi.CommandText = "select * from [dbo].[YMASRS_Formhane_Kon_Sayac_Adresleri]"
        okumaBaglantisayi.Open()
        OkumaKomutSayi.Connection = okumaBaglantisayi
        OkumaKomutSayi.CommandType = CommandType.Text
        Dim okunan2 As SqlDataReader
        okunan2 = OkumaKomutSayi.ExecuteReader()
        Dim i As Integer

        i = 1

        While okunan2.Read()

                sayacadresi(i) = (okunan2("adres"))
                Masa(i) = (okunan2("MakineNumarasi"))
                Konveyor(i) = (okunan2("Konveyor"))
                i += 1
            End While
            okumaBaglantisayi.Close()

    End Sub
End Class

