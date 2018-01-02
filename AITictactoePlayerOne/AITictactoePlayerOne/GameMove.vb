Public Class GameMove
    Public Property x As Integer
    Public Property y As Integer
    Public Property player As Integer

    Public Sub New(inx As Integer, iny As Integer, inplayer As Integer)
        x = inx
        y = iny
        player = inplayer
    End Sub
End Class
