Imports FourkeyCripto

Public Class Util

    Private TCrip = New FourkeyCripto.Cripto()

    Public Function Decifra(ByVal Texto As String)
        Return TCrip.DeCifraTexto(Texto).ToString()
    End Function

    Public Function Cifra(ByVal Texto As String)
        Return TCrip.CifraTexto(Texto).ToString()
    End Function

End Class
