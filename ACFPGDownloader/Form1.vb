Imports System.IO
Imports System.Text
Imports System.Threading
Imports System.Web
Imports System.Net
Imports System.Security.Cryptography

Public Class Form1
    Private SaveDirectory As String = ""
    Private RndGenerator As New Random()
    Private Th As New Thread(AddressOf BeginSaveImgs)
    Private Const DownloadURI = "http://cover.acfunwiki.org/cover.php"
    Private Delegate Sub ByteDeleg(ByRef i() As Byte)

    Private Sub Button1_Click(sender As Object, e As EventArgs) Handles Button1.Click
        Dim fd As New FolderBrowserDialog
        Dim fdr = fd.ShowDialog()
        If fdr = Windows.Forms.DialogResult.Cancel Then Exit Sub
        Dim Folder = fd.SelectedPath
        If Not Folder.EndsWith("\") Then Folder += "\"
        SaveDirectory = Folder
        Th.Start()
    End Sub

    Private Sub BeginSaveImgs()
        On Error Resume Next
        While 1
            Dim wDownloader As New WebClient()
            Dim wData() As Byte = wDownloader.DownloadData(DownloadURI)
            Me.Invoke(New ByteDeleg(AddressOf ShowImage), wData)
            wDownloader.Dispose()
            Thread.Sleep(1000)
        End While
    End Sub
    ''' <summary>
    ''' Judge Extension by Header section
    ''' </summary>
    ''' <param name="input"></param>
    ''' <returns></returns>
    ''' <remarks>
    ''' JPG: FF D8
    ''' TGA: 00 00 02 00 00 |OR| 00 00 10 00 00
    ''' PNG: 89 50 4E 47 0D 0A 1A 0A
    ''' GIF: 47 49 46 38 39 61 |OR| 47 49 46 38 37 61
    ''' BMP: 42 4D
    ''' PCX: 0A
    ''' TIF: 4D 4D |OR| 49 49
    ''' ICO: 00 00 01 00 01 00 20 20
    ''' CUR: 00 00 02 00 01 00 20 20
    ''' IFF: 46 4F 52 4D
    ''' ANI: 52 49 46 46
    ''' </remarks>
    Private Function GetExtensionByByte(ByVal input() As Byte)
        Dim HeaderBytes() As Byte = input.ToList().Take(8).ToArray
        Select Case HeaderBytes(0)
            Case &HFF
                Return "jpg"
            Case &H0
                Select Case HeaderBytes(2)
                    Case &H2
                        Select Case HeaderBytes(4)
                            Case &H1
                                Return "cur"
                            Case &H0
                                Return "tga"
                        End Select
                    Case &H1
                        Return "ico"
                    Case &H10
                        Return "tga"
                End Select
            Case &H89
                If HeaderBytes(1) = &H50 And HeaderBytes(2) = &H4E Then
                    Return "png"
                End If
            Case &H47
                Return "gif"
            Case &H42
                Return "bmp"
            Case &HA
                Return "pcx"
            Case &H4D
                Return "tif"
            Case &H49
                Return "tif"
            Case &H46
                Return "iff"
            Case &H52
                Return "ani"
        End Select

    End Function

    Private Sub ShowImage(ByRef imgBytes() As Byte)
        If imgBytes.Length = 0 Then Exit Sub
        PictureBox1.Image = Image.FromStream(New MemoryStream(imgBytes))
        Dim md5Ret As String = Bytes_To_String(MD5.Create.ComputeHash(imgBytes)).ToLower
        Dim FileName, FullFileName As String
        Dim Extension As String = GetExtensionByByte(imgBytes)
        FileName = md5Ret + "." + Extension
        FullFileName = SaveDirectory + FileName
        If File.Exists(FullFileName) Then
            Thread.Sleep(1000)
            Exit Sub
        End If
        Dim wf = File.Create(FullFileName)
        wf.Write(imgBytes, 0, imgBytes.Length)
        wf.Flush()
        wf.Close()
    End Sub
    Private Function Bytes_To_String(ByVal bytes_Input As Byte()) As String
        Dim strTemp As New StringBuilder(bytes_Input.Length * 2)
        For Each b As Byte In bytes_Input
            strTemp.Append(Conversion.Hex(b))
        Next
        Return strTemp.ToString()
    End Function

    Private Sub Button2_Click(sender As Object, e As EventArgs) Handles Button2.Click
        Application.Exit()
    End Sub
    Protected Overrides Sub OnClosing(e As System.ComponentModel.CancelEventArgs)
        Application.Exit()
        MyBase.OnClosing(e)
    End Sub

End Class
