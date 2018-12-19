Imports HtmlAgilityPack
Public Class Form1
    Public cancelled = 0
    Private Sub Button1_Click(sender As Object, e As EventArgs) Handles Button1.Click
        Label2.Text = "Status: Identifying link"
        If TextBox1.Text.Contains("https://nptel.ac.in/courses/nptel_download.php") Then
            scrap_page(TextBox1.Text)
        ElseIf TextBox1.Text.Contains("https://nptel.ac.in/courses") And Not TextBox1.Text.Contains("/nptel_download.php") Then
            Dim temp As String() = TextBox1.Text.Replace("https://", "").Split("/")
            Dim subid = temp(2)
            scrap_page("https://nptel.ac.in/courses/nptel_download.php?subjectid=" + subid)
        Else
            Label2.Text = "Status: Invalid Link!"
            MsgBox("Doesn't seem like the nptel video downloading link", 16, "Sorry!!")
            TextBox1.Text = ""
            Exit Sub
        End If
    End Sub
    Function scrap_page(ByVal link As String)
        If CheckBox1.Checked Then
            ListView1.Items.Clear()
        End If
        Label2.Text = "Status: Searching for videos"
        Try
            Dim Web As HtmlWeb = New HtmlWeb()
            Dim doc As HtmlDocument
            Dim listitem As ListViewItem
            Dim items(4) As String
            doc = Web.Load(link)
            For Each linkx As HtmlNode In doc.DocumentNode.SelectNodes("//a")
                If linkx.InnerText.ToLower.Contains(ComboBox1.Text.ToLower + " download") Then
                    items(0) = lect(linkx.Attributes("href").Value)
                    items(1) = "https://nptel.ac.in" + linkx.Attributes("href").Value
                    items(2) = linkx.InnerText.Replace(" Download", "")
                    items(3) = "Not Downloaded"
                    listitem = New ListViewItem(items)
                    listitem.Checked = True
                    ListView1.Items.Add(listitem)
                End If
            Next
        Catch ex As Exception
            Label2.Text = "Status: Unable to search Check Internet"
            Return vbOK
            Exit Function
        End Try
        Label2.Text = "Status: Search Completed"
        Return vbOK
    End Function
    Function lect(ByVal urls As String)
        Dim lect_title = "lecture"
        Dim params As String() = urls.Split("&")
        For Each param As String In params
            If param.Contains("subjectName=") Then
                lect_title = param.Replace("subjectName=", "")
            End If
        Next
        Return lect_title
    End Function
    Private Sub LinkLabel1_LinkClicked(sender As Object, e As LinkLabelLinkClickedEventArgs) Handles LinkLabel1.LinkClicked
        MsgBox("By Pratheesh Russell.S", 0, "About")
    End Sub

    Private Sub Button2_Click(sender As Object, e As EventArgs) Handles Button2.Click
        Label2.Text = "Status: Download Protocol Started"
        Dim scroller = 0
        For Each item As ListViewItem In ListView1.CheckedItems
            Dim f2 As New Form2
            Dim x As System.Windows.Forms.DialogResult
            f2.TextBox1.Text = item.SubItems(1).Text
            Label2.Text = "Status: Downloading " + item.SubItems(0).Text
            item.EnsureVisible()
            x = f2.ShowDialog()
            If x = System.Windows.Forms.DialogResult.Cancel Then
                item.BackColor = Color.Red
                item.SubItems(3).Text = "Cancelled"
            ElseIf x = System.Windows.Forms.DialogResult.OK Then
                item.BackColor = Color.Green
                item.SubItems(3).Text = "Downloaded"
            End If
            If cancelled = 1 Then
                Exit For
            End If
            System.Threading.Thread.Sleep(5000)
            scroller += 1
        Next
        If cancelled = 1 Then
            For Each item As ListViewItem In ListView1.CheckedItems
                If item.SubItems(3).Text = "Not Downloaded" Then
                    item.BackColor = Color.Red
                    item.SubItems(3).Text = "Aborted"
                End If
            Next
        End If
        Label2.Text = "Status: Download Protocol Finished"
        cancelled = 0
    End Sub
    Private Sub CopyLinkToolStripMenuItem_Click(sender As Object, e As EventArgs) Handles CopyLinkToolStripMenuItem.Click
        For Each item As ListViewItem In ListView1.SelectedItems
            My.Computer.Clipboard.SetText(item.SubItems(1).Text)
        Next
        Label2.Text = "Status: Item Copied"
    End Sub

    Private Sub SelectAllToolStripMenuItem_Click(sender As Object, e As EventArgs) Handles SelectAllToolStripMenuItem.Click
        For Each item As ListViewItem In ListView1.Items
            item.Checked = True
        Next
    End Sub

    Private Sub SelectNoneToolStripMenuItem_Click(sender As Object, e As EventArgs) Handles SelectNoneToolStripMenuItem.Click
        For Each item As ListViewItem In ListView1.Items
            item.Checked = False
        Next
    End Sub
End Class
