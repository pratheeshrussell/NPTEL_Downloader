Imports System.Net

Public Class Form2
    Dim whereToSave As String 'Where the program save the file
    Dim is_downloaded = False
    Delegate Sub ChangeTextsSafe(ByVal length As Long, ByVal position As Integer, ByVal speed As Double)
    Delegate Sub DownloadCompleteSafe(ByVal cancelled As Boolean)
    Private Sub mainForm_Load(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles MyBase.Load
        Dim filename = ""
        Dim path = ""
        Me.Label4.Text = "Status:"
        Me.TextBox1.Enabled = False
        Dim temp As String() = TextBox1.Text.Split("?")
        Dim params As String() = temp(1).Split("&")
        For Each param As String In params
            If param.Contains("filename=") Then
                filename = param.Replace("filename=", "")
            End If
            If param.Contains("subjectId=") Then
                path = param.Replace("subjectId=", "")
            End If
            If param.Contains("subjectName=") Then
                Me.Text = param.Replace("subjectName=", "")
            End If
        Next
        filename = filename.Replace("." + Form1.ComboBox1.Text, "")
        filename = filename + " " + Me.Text + "." + Form1.ComboBox1.Text 'naming

        Me.Label5.Text = "Downloading: " + filename
        Me.Label6.Text = "Save to: " + path + "/" + filename
        Me.Label3.Text = "File size: Calculating..."
        Me.Label2.Text = "Download speed: Calculating..."
        If (Not System.IO.Directory.Exists(CurDir() + "/" + path)) Then
            System.IO.Directory.CreateDirectory(CurDir() + "/" + path)
        End If
        Me.whereToSave = path + "/" + filename
        Me.BackgroundWorker1.RunWorkerAsync()
    End Sub

    Public Sub DownloadComplete(ByVal cancelled As Boolean)
        is_downloaded = True
        Me.Button1.Enabled = True
        Me.Button2.Enabled = False
        If cancelled Then
            Me.Label4.Text = "Cancelled"
            Me.DialogResult = DialogResult.Cancel
        Else
            Me.Label4.Text = "Successfully downloaded"
            Me.DialogResult = DialogResult.OK
        End If
        Me.Close()
    End Sub

    Public Sub ChangeTexts(ByVal length As Long, ByVal position As Integer, ByVal speed As Double)
        Me.Label3.Text = "File Size: " & Math.Round((length / 1024), 2) & " KB"
        If speed = -1 Then
            Me.Label2.Text = "Speed: calculating..."
        Else
            Me.Label2.Text = "Speed: " & Math.Round((speed / 1024), 2) & " KB/s"
        End If
        Me.ProgressBar1.Value = (Math.Round((position / 1024), 2) * 100) / Math.Round((length / 1024), 2) 'percent
        Me.Label4.Text = "Downloaded " & Math.Round((position / 1024), 2) & " KB of " & Math.Round((length / 1024), 2) & "KB (" & Me.ProgressBar1.Value & "%)"
    End Sub
    Private Sub BackgroundWorker1_DoWork(ByVal sender As System.Object, ByVal e As System.ComponentModel.DoWorkEventArgs) Handles BackgroundWorker1.DoWork
        'Creating the request and getting the response
        Dim theResponse As HttpWebResponse
        Dim theRequest As HttpWebRequest
        Try 'Checks if the file exist
            theRequest = WebRequest.Create(Me.TextBox1.Text)
            theResponse = theRequest.GetResponse
        Catch ex As Exception
            ' MessageBox.Show("An error occurred while downloading file. Possibe causes:" & ControlChars.CrLf &
            '                "1) File doesn't exist" & ControlChars.CrLf &
            '                "2) Remote server error", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error)
            Dim cancelDelegate As New DownloadCompleteSafe(AddressOf DownloadComplete)
            Me.Invoke(cancelDelegate, True)
            Exit Sub
        End Try
        Dim length As Long = theResponse.ContentLength 'Size of the response (in bytes)
        Dim safedelegate As New ChangeTextsSafe(AddressOf ChangeTexts)
        Me.Invoke(safedelegate, length, 0, 0) 'Invoke the TreadsafeDelegate
        Dim writeStream As New IO.FileStream(Me.whereToSave, IO.FileMode.Create)
        'Replacement for Stream.Position (webResponse stream doesn't support seek)
        Dim nRead As Integer
        'To calculate the download speed
        Dim speedtimer As New Stopwatch
        Dim currentspeed As Double = -1
        Dim readings As Integer = 0
        Do
            If BackgroundWorker1.CancellationPending Then 'If user abort download
                Exit Do
            End If
            speedtimer.Start()
            Dim readBytes(4095) As Byte
            Dim bytesread As Integer = theResponse.GetResponseStream.Read(readBytes, 0, 4096)
            nRead += bytesread
            Me.Invoke(safedelegate, length, nRead, currentspeed)
            If bytesread = 0 Then Exit Do
            writeStream.Write(readBytes, 0, bytesread)
            speedtimer.Stop()
            readings += 1
            If readings >= 5 Then 'For increase precision, the speed it's calculated only every five cicles
                currentspeed = 20480 / (speedtimer.ElapsedMilliseconds / 1000)
                speedtimer.Reset()
                readings = 0
            End If
        Loop
        'Close the streams
        theResponse.GetResponseStream.Close()
        writeStream.Close()
        If Me.BackgroundWorker1.CancellationPending Then
            IO.File.Delete(Me.whereToSave)
            Dim cancelDelegate As New DownloadCompleteSafe(AddressOf DownloadComplete)
            Me.Invoke(cancelDelegate, True)
            Exit Sub
        End If
        Dim completeDelegate As New DownloadCompleteSafe(AddressOf DownloadComplete)
        Me.Invoke(completeDelegate, False)
    End Sub
    Private Sub btnCancel_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles Button2.Click
        Me.BackgroundWorker1.CancelAsync() 'Send cancel request
    End Sub

    Private Sub Button1_Click(sender As Object, e As EventArgs) Handles Button1.Click
        Form1.cancelled = 1
        Me.BackgroundWorker1.CancelAsync() 'Send cancel request
    End Sub
    Private Sub mainForm_close(ByVal sender As System.Object, ByVal e As System.Windows.Forms.FormClosingEventArgs) Handles MyBase.FormClosing
        If is_downloaded = False Then
            e.Cancel = True
        End If
    End Sub
End Class

