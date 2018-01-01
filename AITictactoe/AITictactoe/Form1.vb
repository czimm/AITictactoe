Imports System.ComponentModel
Imports System.Net

Public Class Form1

    Private Startx As Integer = 50
    Private Starty As Integer = 50
    Private xSize As Integer = 3
    Private ySize As Integer = 3
    Private CurrentPlayerTurn As Integer = 1
    Private CurrentWinner As Integer = 0
    Private CurrentMoves As New List(Of GameMove)
    Private IsListening As Boolean = True
    Private weblistener As System.Net.HttpListener = Nothing

    Private Sub Form1_Load(sender As Object, e As EventArgs) Handles Me.Load
        Dim t As New Threading.Thread(AddressOf StartWebServer)
        t.Start()
    End Sub

    Private Sub Form1_Paint(sender As Object, e As PaintEventArgs) Handles Me.Paint
        Dim g As System.Drawing.Graphics = Me.CreateGraphics
        ' Dim Rectangle As System.Drawing.Rectangle = New System.Drawing.Rectangle(10, 10, 200, 200)


        Dim Pen As New Pen(Color.Black, 2)
        For x As Integer = 0 To xSize - 1
            For y As Integer = 0 To ySize - 1
                g.DrawRectangle(Pen, Startx + (x * 40), Starty + (y * 40), 40, 40)
            Next
        Next
        'g.DrawRectangle(Pen, 10, 10, 20, 20)

        'Dim BluePen As New SolidBrush(Color.Blue)
        'g.FillEllipse(BluePen, New System.Drawing.Rectangle(51, 51, 37, 37))
        'Dim RedPen As New SolidBrush(Color.Red)
        ' g.FillEllipse(RedPen, New System.Drawing.Rectangle(10, 10, 200, 200))
        ' Graphics.DrawRectangle(System.Drawing.Pens.Black, Rectangle)
        For Each dtl As GameMove In CurrentMoves
            SetButton(dtl, False)
        Next
        Pen.Dispose()
    End Sub

    Private Sub Button1_Click(sender As Object, e As EventArgs) Handles Button1.Click
        SetButton(New GameMove(1, 3, 1), True)
        SetButton(New GameMove(2, 2, 1), True)
        SetButton(New GameMove(3, 1, 1), True)
    End Sub
    Private Sub StartWebServer()
        If Not System.Net.HttpListener.IsSupported Then
            Console.WriteLine(
            "Windows XP SP2, Server 2003, or higher is required to " &
            "use the HttpListener class.")
            Exit Sub
        End If
        ' URI prefixes are required,
        ' Create a listener and add the prefixes.
        Dim listener As System.Net.HttpListener = New System.Net.HttpListener()
        listener.Prefixes.Add("http://*:8000/")
        'netsh http add urlacl url=http://*:8000/ user=Everyone

        Try
            ' Start the listener to begin listening for requests.
            listener.Start()
            Console.WriteLine("Listening...")
            weblistener = listener
            ' Set the number of requests this application will handle.
            ' Dim IsListening As Boolean = True
            Do Until IsListening = False
                Dim response As HttpListenerResponse = Nothing
                Try
                    ' Note: GetContext blocks while waiting for a request.
                    Dim context As HttpListenerContext = listener.GetContext
                    ' Create the response.
                    '  PrintOpenTicketQueue()
                    response = context.Response
                    Dim responseString As String = "Request Error"
                    If context.Request.Url.Segments(1).Replace("/", "") = "getgamestate" Then
                        'Return Game State
                        Dim state As New GameState()
                        state.CurrentMoves = CurrentMoves
                        state.PlayerTurn = CurrentPlayerTurn
                        state.PlayerWin = CurrentWinner
                        responseString = Newtonsoft.Json.JsonConvert.SerializeObject(state, Newtonsoft.Json.Formatting.None)
                    ElseIf context.Request.Url.Segments(1).Replace("/", "") = "gamemove" AndAlso context.Request.HttpMethod = "POST" Then
                        Dim reader As New IO.StreamReader(context.Request.InputStream)
                        Dim json As String = reader.ReadToEnd()
                        Dim gm As GameMove = Newtonsoft.Json.JsonConvert.DeserializeObject(Of GameMove)(json)
                        If gm IsNot Nothing AndAlso gm.x > 0 AndAlso gm.y > 0 AndAlso gm.player > 0 Then
                            Dim found As Boolean = False
                            For Each dtl As GameMove In CurrentMoves
                                If dtl.x = gm.x AndAlso dtl.y = gm.y Then
                                    found = True
                                End If
                            Next
                            If found Then
                                responseString = "Move Already Played"
                            ElseIf CurrentWinner > 0 Then
                                responseString = "Game Over"
                            Else
                                SetButton(gm, True)
                                responseString = "Move Complete"
                            End If
                        End If
                    End If


                        Dim buffer() As Byte = System.Text.Encoding.UTF8.GetBytes(responseString)
                    response.ContentLength64 = buffer.Length
                    Dim output As System.IO.Stream = response.OutputStream
                    output.Write(buffer, 0, buffer.Length)
                Catch ex As HttpListenerException
                    Console.WriteLine(ex.Message)
                Finally
                    If response IsNot Nothing Then
                        response.Close()
                    End If
                End Try
            Loop
        Catch ex As HttpListenerException
            Console.WriteLine(ex.Message)
        Finally
            ' Stop listening for requests.
            listener.Close()
            Console.WriteLine("Done Listening...")
        End Try
    End Sub
    Private Sub SetButton(gm As GameMove, AddToMoves As Boolean)
        If AddToMoves Then
            CurrentMoves.Add(gm)
        End If
        Dim g As System.Drawing.Graphics = Me.CreateGraphics

        Dim Pen As SolidBrush = Nothing
        If gm.player = 1 Then
            Pen = New SolidBrush(Color.Blue)
            CurrentPlayerTurn = 2
        ElseIf gm.player = 2 Then
            Pen = New SolidBrush(Color.Red)
            CurrentPlayerTurn = 1
        End If
        Dim x As Integer = gm.x - 1
        Dim y As Integer = gm.y - 1
        g.FillEllipse(Pen, New System.Drawing.Rectangle(Startx + (x * 40) + 1, Starty + (y * 40) + 1, 37, 37))

        CurrentWinner = CheckforWin()
        If CurrentWinner > 0 Then
            Label1.Text = "Player " & CurrentWinner & " Wins!!!"
            Label1.Visible = True
        End If
    End Sub
    Private Function CheckforWin() As Integer

        Dim wincount As Integer = 0
        Dim tempplayer As Integer = 0
        'checkx
        For x As Integer = 1 To xSize
            For y As Integer = 1 To ySize
                For Each dtl As GameMove In CurrentMoves
                    If dtl.x = x AndAlso dtl.y = y Then
                        If tempplayer = dtl.player Then
                            wincount += 1
                        End If
                        If wincount >= 2 Then
                            Return tempplayer
                        End If
                        tempplayer = dtl.player
                    End If

                Next
            Next
            wincount = 0
            tempplayer = 0
        Next

        'checky
        wincount = 0
        tempplayer = 0
        For y As Integer = 1 To ySize
            For x As Integer = 1 To xSize
                For Each dtl As GameMove In CurrentMoves
                    If dtl.x = x AndAlso dtl.y = y Then
                        If tempplayer = dtl.player Then
                            wincount += 1
                        End If
                        If wincount >= 2 Then
                            Return tempplayer
                        End If
                        tempplayer = dtl.player
                    End If

                Next
            Next
            wincount = 0
            tempplayer = 0
        Next

        'diagdown
        wincount = 0
        tempplayer = 0
        For x As Integer = xSize To 1 Step -1
            Dim y As Integer = 1
            For count As Integer = 0 To xSize - 1
                For Each dtl As GameMove In CurrentMoves
                    If dtl.x = x + count AndAlso dtl.y = y + count Then
                        If tempplayer = dtl.player Then
                            wincount += 1
                        End If
                        If wincount >= 2 Then
                            Return tempplayer
                        End If
                        tempplayer = dtl.player
                    End If
                Next
            Next
            wincount = 0
                tempplayer = 0
            Next
            wincount = 0
        tempplayer = 0
        For y As Integer = 1 To ySize
            Dim x As Integer = 1
            For count As Integer = 0 To ySize - 1
                For Each dtl As GameMove In CurrentMoves
                    If dtl.x = x + count AndAlso dtl.y = y + count Then
                        If tempplayer = dtl.player Then
                            wincount += 1
                        End If
                        If wincount >= 2 Then
                            Return tempplayer
                        End If
                        tempplayer = dtl.player
                    End If

                Next
            Next
            wincount = 0
            tempplayer = 0
        Next

        'diagup
        wincount = 0
        tempplayer = 0
        For y As Integer = 1 To ySize
            Dim x As Integer = 1
            For count As Integer = 0 To ySize - 1
                For Each dtl As GameMove In CurrentMoves
                    If dtl.x = x + count AndAlso dtl.y = y - count Then
                        If tempplayer = dtl.player Then
                            wincount += 1
                        End If
                        If wincount >= 2 Then
                            Return tempplayer
                        End If
                        tempplayer = dtl.player
                    End If

                Next
            Next
            wincount = 0
            tempplayer = 0
        Next
        wincount = 0
        tempplayer = 0
        For x As Integer = xSize To 1 Step -1
            Dim y As Integer = xSize
            For count As Integer = 0 To xSize - 1
                For Each dtl As GameMove In CurrentMoves
                    If dtl.x = x + count AndAlso dtl.y = y - count Then
                        If tempplayer = dtl.player Then
                            wincount += 1
                        End If
                        If wincount >= 2 Then
                            Return tempplayer
                        End If
                        tempplayer = dtl.player
                    End If
                Next
            Next
            wincount = 0
            tempplayer = 0
        Next

        Return 0
    End Function
    Private Sub Form1_Closing(sender As Object, e As CancelEventArgs) Handles Me.Closing
        IsListening = False
        If weblistener IsNot Nothing Then
            weblistener.Stop()
        End If
    End Sub
End Class
