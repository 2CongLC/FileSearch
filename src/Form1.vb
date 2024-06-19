Imports System
Imports System.Collections
Imports System.ComponentModel
Imports System.Diagnostics
Imports System.IO
Imports System.IO.Compression
Imports System.Net
Imports System.Net.Sockets
Imports System.Net.WebSockets
Imports System.Reflection
Imports System.Text
Imports System.Text.RegularExpressions
Imports System.Threading
Imports Microsoft.Office.Interop

Public Class Form1
    Private IsRun As Boolean

#Region "Khu vực xử lý Form1"
    Private Sub Form1_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        NotifyIcon1.Visible = False
    End Sub
    Private Sub Form1_Resize(sender As Object, e As EventArgs) Handles Me.Resize
        If Me.WindowState = FormWindowState.Minimized Then
            Me.ShowInTaskbar = False
            If IsRun = True Then
                NotifyIcon1.Icon = Icon.FromHandle(CType(ImageList1.Images(1), Bitmap).GetHicon)
            Else
                NotifyIcon1.Icon = Icon.FromHandle(CType(ImageList1.Images(0), Bitmap).GetHicon)
            End If
            NotifyIcon1.Visible = True
            BalloonTip("FileSeach - Phần mềm tìm kiếm và quản lí tệp tin", "Liên hệ : 2conglc.vn@gmail.com", ToolTipIcon.None)
        End If
    End Sub
    Private Sub Button1_Click(sender As Object, e As EventArgs) Handles Button1.Click
        If Directory.Exists(TextBox2.Text) Then
            Start_Stop()
        End If
    End Sub
    Private Sub Button2_Click(sender As Object, e As EventArgs) Handles Button2.Click
        If FolderBrowserDialog1.ShowDialog = DialogResult.OK Then
            TextBox2.Text = FolderBrowserDialog1.SelectedPath
        End If
    End Sub
#End Region
#Region "Khu vực xử lí NotifyIcon1"
    Private Sub NotifyIcon1_MouseDoubleClick(sender As Object, e As MouseEventArgs) Handles NotifyIcon1.MouseDoubleClick
        UnhideProcess()
    End Sub

    Private Sub NotifyIcon1_MouseClick(sender As Object, e As MouseEventArgs) Handles NotifyIcon1.MouseClick
        If e.Button = MouseButtons.Left Then
            Start_Stop()
            If IsRun = True Then
                NotifyIcon1.Icon = Icon.FromHandle(CType(ImageList1.Images(1), Bitmap).GetHicon)
            Else
                NotifyIcon1.Icon = Icon.FromHandle(CType(ImageList1.Images(0), Bitmap).GetHicon)
            End If
        End If
    End Sub
    Private Sub BalloonTip(title As String, text As String, icon As ToolTipIcon)
        NotifyIcon1.BalloonTipTitle = title
        NotifyIcon1.BalloonTipText = text
        NotifyIcon1.BalloonTipIcon = icon
        NotifyIcon1.ShowBalloonTip(5000) '10000
    End Sub
    Private Sub ShowFormToolStripMenuItem_Click(sender As Object, e As EventArgs) Handles ShowFormToolStripMenuItem.Click
        UnhideProcess()
    End Sub
    Private Sub ThoatToolStripMenuItem_Click(sender As Object, e As EventArgs) Handles ThoatToolStripMenuItem.Click
        If IsRun = True Then
            BackgroundWorker1.CancelAsync()
            IsRun = False
        End If
        Me.Close()
    End Sub
#End Region
#Region "Hiện/ Ẩn xuống khay hệ thống"
    Private Sub HideProcess()
        Me.WindowState = FormWindowState.Minimized
        Me.ShowInTaskbar = False
        NotifyIcon1.Visible = True
    End Sub
    Private Sub UnhideProcess()
        Me.WindowState = FormWindowState.Normal
        Me.ShowInTaskbar = True
        NotifyIcon1.Visible = True
    End Sub
#End Region
#Region "Tìm kiếm File"
    Private Sub Start_Stop()
        If BackgroundWorker1.IsBusy AndAlso IsRun = True Then
            BackgroundWorker1.CancelAsync()
            IsRun = False
        Else
            ToolStripProgressBar1.Value = ToolStripProgressBar1.Minimum
            Button1.Text = "Dừng lại"
            ListView1.Items.Clear()
            BackgroundWorker1.RunWorkerAsync()
            IsRun = True
        End If
    End Sub

    Private Sub AddToListView(ByVal file As String)
        Dim finfo As FileInfo = New FileInfo(file)
        Dim item As ListViewItem = New ListViewItem(finfo.Name)
        item.SubItems.Add(finfo.DirectoryName)
        item.SubItems.Add(Math.Ceiling(finfo.Length / 1024.0F).ToString("0 KB"))
        item.SubItems.Add(finfo.Attributes.ToString)
        item.SubItems.Add(finfo.CreationTimeUtc)
        ListView1.Invoke(CType((Sub()
                                    ListView1.BeginUpdate()
                                    ListView1.Items.Add(item)
                                    ListView1.EndUpdate()
                                End Sub), Action))
    End Sub
    ''' <summary>
    ''' Tìm kiếm tệp tin
    ''' </summary>
    ''' <param name="dt"></param>
    ''' <param name="searchPattern"></param>
    ''' <returns></returns>
    Public Function ScanDirectory(ByVal dt As String, ByVal searchPattern As String) As List(Of String)
        Dim list As New List(Of String)
        Try
            If Directory.Exists(dt) AndAlso searchPattern <> "" Then
                Dim list2 As List(Of String) = Enumerable.ToList(Of String)(Directory.GetFiles(dt, searchPattern, SearchOption.AllDirectories))
                Dim num As Integer = 1
                Dim i As Integer
                For i = 0 To list2.Count - 1
                    Application.DoEvents()
                    Dim info As New FileInfo(list2(i))
                    list.Add(list2(i))
                    num += 1
                Next i
            End If
            Return list
        Catch ex As Exception

            MessageBox.Show("Bạn cần chạy quyền Administrator")
        End Try
    End Function
#End Region
#Region "xử lí BackgroundWorker1"
    Private Sub BackgroundWorker1_DoWork(sender As Object, e As ComponentModel.DoWorkEventArgs) Handles BackgroundWorker1.DoWork
        Try
            Dim dirs As New List(Of String)
            dirs = ScanDirectory(TextBox2.Text, TextBox1.Text)
            Dim count As Integer = dirs.Count
            ListView1.Items.Clear()
            Dim i As Integer
            For i = 0 To count - 1
                'Tham chiếu tiến trình progressbar
                BackgroundWorker1.ReportProgress(CInt(i / count * 100))
                'Tham chiếu tệp đang trong quá trình sử lý
                Label3.Invoke(CType((Sub()
                                         Label3.Text = String.Format("Đang đếm trang : {0}", dirs(i))
                                     End Sub), Action))
                'Thêm tệp tin vào danh sách listview1
                AddToListView(dirs(i))
                'Tiến trình đếm trang
                ToolStripStatusLabel3.Text = "Kết quả tìm được : " & ListView1.Items.Count
            Next
            BackgroundWorker1.ReportProgress(100)
        Catch ex As Exception
        End Try
    End Sub

    Private Sub BackgroundWorker1_ProgressChanged(sender As Object, e As ProgressChangedEventArgs) Handles BackgroundWorker1.ProgressChanged
        If Not BackgroundWorker1.CancellationPending Then
            ToolStripStatusLabel2.Text = e.ProgressPercentage & "%"
            NotifyIcon1.Text = "Tiến độ tìm kiếm : " & e.ProgressPercentage & "%"
            ToolStripProgressBar1.PerformStep()
        End If
    End Sub

    Private Sub BackgroundWorker1_RunWorkerCompleted(sender As Object, e As RunWorkerCompletedEventArgs) Handles BackgroundWorker1.RunWorkerCompleted
        If ToolStripProgressBar1.Value = ToolStripProgressBar1.Maximum Then
            Label3.Text = "Đã tìm xong"
            Dim result As String = String.Format("Kết quả : {0} tệp đã được tìm thấy", ListView1.Items.Count)
            BalloonTip("Đã tìm xong", result, ToolTipIcon.Info)
            NotifyIcon1.Icon = Icon.FromHandle(CType(ImageList1.Images(0), Bitmap).GetHicon)
            NotifyIcon1.Text = Me.Text
            Button1.Text = "Tìm kiếm"
            UnhideProcess()
        End If
    End Sub
#End Region
#Region "Quản lí tệp tìm được"
    Private Function GetRowListView(id As Integer) As String
        Dim result As String = ""
        result = ListView1.Items.Item(ListView1.FocusedItem.Index).SubItems.Item(id).Text
        Return result
    End Function

    Private Sub MơThưMucChưaTêpToolStripMenuItem_Click(sender As Object, e As EventArgs) Handles MơThưMucChưaTêpToolStripMenuItem.Click
        Process.Start("explorer.exe", "/select," & GetRowListView(1) & "\" & GetRowListView(0))
    End Sub

    Private Sub DichuyenteptinToolStripMenuItem_Click(sender As Object, e As EventArgs) Handles DichuyenteptinToolStripMenuItem.Click
        If SaveFileDialog1.ShowDialog = DialogResult.OK Then
            File.Move(GetRowListView(1) & "\" & GetRowListView(0), SaveFileDialog1.FileName)
            ListView1.Items(ListView1.FocusedItem.Index).Remove()
            BalloonTip("Đã di chuyển tệp tin tới", SaveFileDialog1.FileName, ToolTipIcon.Info)
        End If
    End Sub

    Private Sub SaoChepTêpTinToolStripMenuItem_Click(sender As Object, e As EventArgs) Handles SaoChepTêpTinToolStripMenuItem.Click
        If SaveFileDialog1.ShowDialog = DialogResult.OK Then
            File.Copy(GetRowListView(1) & "\" & GetRowListView(0), SaveFileDialog1.FileName)
            BalloonTip("Đã sao chép tệp tin tới", SaveFileDialog1.FileName, ToolTipIcon.Info)
        End If
    End Sub

    Private Sub XoaTêpTinToolStripMenuItem_Click(sender As Object, e As EventArgs) Handles XoaTêpTinToolStripMenuItem.Click
        If File.Exists(GetRowListView(1) & "\" & GetRowListView(0)) Then
            File.Delete(GetRowListView(1) & "\" & GetRowListView(0))
            ListView1.Items(ListView1.FocusedItem.Index).Remove()
            BalloonTip("Đã xóa tệp tin", GetRowListView(1) & "\" & GetRowListView(0), ToolTipIcon.Warning)
        End If
    End Sub
    Private Sub ExcelToolStripMenuItem_Click(sender As Object, e As EventArgs) Handles ExcelToolStripMenuItem.Click
        Try
            SaveFileDialog2.Filter = "Excel File (*.xlsx) | *.xlsx"
            If SaveFileDialog2.ShowDialog = DialogResult.OK Then
                Dim app As Excel.Application = New Excel.Application
                Dim wb As Excel.Workbook = app.Workbooks.Add(1)
                Dim ws As Excel.Worksheet = CType(wb.Worksheets(1), Excel.Worksheet)

                'Lấy thông tin header trong cột
                Dim j As Integer = 1
                Dim j2 As Integer = 1
                For Each ch As ColumnHeader In ListView1.Columns
                    ws.Cells(j2, j) = ch.Text
                    j += 1
                Next

                'Lấy nội dung trong cột
                Dim i As Integer = 1
                Dim i2 As Integer = 2

                For Each lvi As ListViewItem In ListView1.Items
                    i = 1
                    For Each lvs As ListViewItem.ListViewSubItem In lvi.SubItems
                        ws.Cells(i2, i) = lvs.Text
                        i = (i + 1)
                    Next
                    i2 = (i2 + 1)
                Next



                'Xuất sang tệp excel
                wb.SaveAs(SaveFileDialog2.FileName, Excel.XlFileFormat.xlOpenXMLWorkbook, Missing.Value,
                                Missing.Value, False, False, Excel.XlSaveAsAccessMode.xlNoChange,
                                Excel.XlSaveConflictResolution.xlUserResolution, True,
                                Missing.Value, Missing.Value, Missing.Value)
                wb.Close(False, Type.Missing, Type.Missing)
                app.Quit()
                MessageBox.Show("Done !")
            End If
        Catch ex As Exception
            MessageBox.Show(ex.ToString())
        End Try
    End Sub

    Private Shared Sub WriteCSVRow(ByVal result As StringBuilder, ByVal itemsCount As Integer, ByVal isColumnNeeded As Func(Of Integer, Boolean), ByVal columnValue As Func(Of Integer, String))
        Dim isFirstTime As Boolean = True

        For i As Integer = 0 To itemsCount - 1
            If Not isColumnNeeded(i) Then Continue For
            If Not isFirstTime Then result.Append(",")
            isFirstTime = False
            result.Append(String.Format("""{0}""", columnValue(i)))
        Next

        result.AppendLine()
    End Sub
    Public Shared Sub ListViewToCSV(ByVal listView As ListView, ByVal filePath As String, ByVal includeHidden As Boolean)
        'make header string
        Dim result As StringBuilder = New StringBuilder()
        WriteCSVRow(result, listView.Columns.Count, Function(i) includeHidden OrElse listView.Columns(i).Width > 0, Function(i) listView.Columns(i).Text)

        'export data rows
        For Each listItem As ListViewItem In listView.Items
            WriteCSVRow(result, listView.Columns.Count, Function(i) includeHidden OrElse listView.Columns(i).Width > 0, Function(i) listItem.SubItems(i).Text)
        Next

        File.WriteAllText(filePath, result.ToString(), Encoding.UTF8)
    End Sub
    Private Sub CSVToolStripMenuItem_Click(sender As Object, e As EventArgs) Handles CSVToolStripMenuItem.Click
        Try
            SaveFileDialog2.Filter = "CSV File (*.csv) | *.csv"
            If SaveFileDialog2.ShowDialog = DialogResult.OK Then
                ListViewToCSV(ListView1, SaveFileDialog2.FileName, True)
                MessageBox.Show("Done !")
            End If
        Catch ex As Exception
            MessageBox.Show(ex.ToString())
        End Try
    End Sub

#End Region

End Class
