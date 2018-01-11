Imports FourkeyCripto
Imports System.Net.NetworkInformation

Public Class Util

    Private TCrip = New FourkeyCripto.Cripto()

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

        If My.Computer.Network.Ping(Decifra(My.Settings.PathFtp)) Then
            Return True
        Else
            Return False
        End If

    End Function

    Public Sub Escreve_Log(ByVal Texto As String)

        Dim fluxoTexto As IO.StreamWriter

        fluxoTexto = New IO.StreamWriter(Frm_Principal.ClientLocalPasta & "\Logs\Admin_Walle.txt", True)
        fluxoTexto.WriteLine("")
        fluxoTexto.WriteLine("--------------------------------------------------------------")

        fluxoTexto.WriteLine(Format(Now, "yyyy-MM-dd HH:mm:ss") & " - " & Texto)

        fluxoTexto.Close()

    End Sub

End Class
