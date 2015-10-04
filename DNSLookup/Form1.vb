Imports System.Net

Public Class Form1
    Private Sub Form1_Load(sender As Object, e As EventArgs) Handles MyBase.Load

    End Sub

    Private Sub Button1_Click(sender As Object, e As EventArgs) Handles Button1.Click
        Dim Options = New JHSoftware.DnsClient.RequestOptions
        Options.DnsServers = New System.Net.IPAddress() {
               System.Net.IPAddress.Parse("114.114.114.114"),
               System.Net.IPAddress.Parse("8.8.8.8")}
        Dim IPs = JHSoftware.DnsClient.LookupHost("www.baidu.com",
                                          JHSoftware.DnsClient.IPVersion.IPv4,
                                          Options)
        For Each IP In IPs
            Console.WriteLine(IP.ToString)
        Next
    End Sub

    Private Sub Button2_Click(sender As Object, e As EventArgs) Handles Button2.Click
        Dim sr As New IO.StreamReader("nameservers.txt")
        For i = 1 To Val(TextBox1.Text) - 1
            sr.ReadLine()
        Next
        Dim count As Integer = Val(TextBox2.Text)
        Dim dns(count) As String
        For i = 1 To count
            dns(i) = sr.ReadLine
        Next
        sr.Close()
        Dim sw As New IO.StreamWriter("dns" & TextBox1.Text & "_" & (Val(TextBox1.Text) + Val(TextBox2.Text) - 1) & ".txt")
        For i = 1 To count
            Try
                Dim Options = New JHSoftware.DnsClient.RequestOptions
                Options.DnsServers = New System.Net.IPAddress() {System.Net.IPAddress.Parse(dns(i))}
                Dim IPs = JHSoftware.DnsClient.LookupHost("www.dropbox.com", JHSoftware.DnsClient.IPVersion.IPv4, Options)
                If IPs.Count() > 0 Then
                    sw.WriteLine(dns(i))
                End If
                For Each IP In IPs
                    Console.WriteLine(i & " " & IP.ToString)
                Next
            Catch ex As Exception
                Debug.WriteLine(dns(i) + " " + ex.ToString())
            End Try
        Next
        sw.Close()
    End Sub

    Private Sub Button3_Click(sender As Object, e As EventArgs) Handles Button3.Click
        Dim sr As New IO.StreamReader("nameservers.txt")
        For i = 1 To Val(TextBox1.Text) - 1
            sr.ReadLine()
        Next
        Dim count As Integer = Val(TextBox2.Text)
        Dim dns(count) As String
        For i = 1 To count
            dns(i) = sr.ReadLine
        Next
        sr.Close()
        Dim ipset As New HashSet(Of String)
        For i = 1 To count
            Try
                Dim Options = New JHSoftware.DnsClient.RequestOptions
                Options.DnsServers = New System.Net.IPAddress() {System.Net.IPAddress.Parse(dns(i))}
                Dim IPs = JHSoftware.DnsClient.LookupHost(TextBox3.Text, JHSoftware.DnsClient.IPVersion.IPv4, Options)
                For Each IP In IPs
                    ipset.Add(IP.ToString)
                Next
            Catch ex As Exception
                Debug.WriteLine(dns(i) + " " + ex.ToString())
            End Try
            Debug.WriteLine(i)
            'Label1.Text = Trim(i)
            'Application.DoEvents()
        Next
        Dim sw As New IO.StreamWriter(TextBox3.Text & " " & TextBox1.Text & "_" & (Val(TextBox1.Text) + Val(TextBox2.Text) - 1) & ".txt")
        For Each ip As String In ipset
            sw.WriteLine(ip)
        Next
        sw.Close()
        MsgBox("ok")
    End Sub
End Class
