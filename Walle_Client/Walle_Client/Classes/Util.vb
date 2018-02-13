Imports FourkeyCripto
Imports System.Net.NetworkInformation
Imports System.IO

Public Class Util

    Private TCrip = New FourkeyCripto.Cripto()
    Dim ContLog As Integer

    Public Function Decifra(ByVal Texto As String)
        Return TCrip.DeCifraTexto(Texto).ToString()
    End Function

    Public Function Cifra(ByVal Texto As String)
        Return TCrip.CifraTexto(Texto).ToString()
    End Function

    ''' <summary>
    ''' Verifica se existe conexão com o FTP
    ''' </summary>
    ''' <returns></returns>
    Public Function VerificaConexaoFtp() As Boolean

        Dim Caminho As String = Replace(Decifra(My.Settings.PathFtp), "ftp://", "")

        If My.Computer.Network.Ping(Caminho) Then
            Return True
        Else
            Return False
        End If

    End Function

    Public Sub Escreve_Log(ByVal Texto As String)

        Dim LogArquivo As String = Frm_Principal.ClientLocalPasta & "\Logs\" & Format(Now, "yyyy_MM_dd") & "_Walle_Client_" & ContLog & ".txt"

        If File.Exists(LogArquivo) = True Then

            Dim file As FileInfo = New FileInfo(LogArquivo)

            If file.Length > 20503120 Then

                ContLog += 1

            End If

        End If

        Dim fluxoTexto As IO.StreamWriter

        fluxoTexto = New IO.StreamWriter(LogArquivo, True)
        fluxoTexto.WriteLine("")

        fluxoTexto.WriteLine(Format(Now, "yyyy-MM-dd HH:mm:ss") & " - " & Texto)

        fluxoTexto.Close()

    End Sub

End Class
