Imports System.IO

Public Class Frm_Auxiliar

    Public PassCriptograf As String
    Public Fechamento As Boolean = False

    Private Sub TextBox1_KeyDown(sender As Object, e As KeyEventArgs) Handles tb_password.KeyDown

        If e.KeyCode = Keys.Enter Then

            If tb_password.Text = "fourkey" Then

                For Each processo As Process In Process.GetProcesses()

                    If processo.ProcessName = "Walle_Client_Data" Then

                        processo.Kill()

                    End If

                Next processo

                Application.Exit()

            Else

                tb_password.Clear()

            End If

        End If

    End Sub

    Private Sub Frm_Auxiliar_Resize(sender As Object, e As EventArgs) Handles MyBase.Resize

        If Me.WindowState = FormWindowState.Minimized Then

            Me.Hide()

        End If

    End Sub

    Private Sub Button1_Click(sender As Object, e As EventArgs) Handles Button1.Click

    End Sub

    Private Sub tb_password_TextChanged(sender As Object, e As EventArgs) Handles tb_password.TextChanged

    End Sub

    Private Sub Frm_Auxiliar_FormClosing(sender As Object, e As FormClosingEventArgs) Handles MyBase.FormClosing

        Dim CaminhoWi As String = System.Reflection.Assembly.GetExecutingAssembly().Location

        CaminhoWi = CaminhoWi.Replace("Walle_Client.exe", "")

        If File.Exists(CaminhoWi & "Windows64.txt") Then

            Fechamento = True

        End If

        If Fechamento = False Then

            e.Cancel = True

        End If

    End Sub

End Class