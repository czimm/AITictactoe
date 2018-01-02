Imports System.Text

Module Program

    Sub Main(args As String())
        Console.WriteLine("Hello World!")
        Dim availablemoves As New List(Of Integer)
        'For i As Integer = 1 To 9
        '    availablemoves.Add(i)
        'Next

        Dim webClient As New System.Net.WebClient
        Dim currentplayer As Integer = 2
        Dim exitapp As Boolean = False
        Dim gameinprogress As Boolean = True

        Do Until exitapp
            'Check Game Stat
            Try
                Dim responsebytes = webClient.DownloadData("http://localhost:8000/getgamestate")
                Dim responsebody = (New Text.UTF8Encoding).GetString(responsebytes)

                If responsebody.IndexOf("{""PlayerTurn"":") >= 0 Then
                    Dim gs As GameState = Newtonsoft.Json.JsonConvert.DeserializeObject(Of GameState)(responsebody)
                    If gs IsNot Nothing AndAlso gs.PlayerWin = 0 Then
                        'Play Game
                        'If currentplayer = 2 Then
                        '    currentplayer = 1
                        'Else
                        '    currentplayer = 2
                        'End If
                        availablemoves.Clear()
                        For i As Integer = 1 To 9
                            availablemoves.Add(i)
                        Next
                        If gs.CurrentMoves.Count > 0 Then
                            For Each dtl As GameMove In gs.CurrentMoves
                                availablemoves.Remove((dtl.y - 1) * 3 + dtl.x)
                            Next
                        End If
                        currentplayer = gs.PlayerTurn

                        Dim r As New Random
                        Dim m As Integer = r.Next(0, availablemoves.Count)

                        Dim y As Integer = Math.Ceiling((availablemoves(m) / 3))
                        Dim x As Integer = availablemoves(m) - ((y - 1) * 3)
                        Console.WriteLine(m & "," & availablemoves(m) & "," & x & "," & y)
                        availablemoves.RemoveAt(m)
                        Dim gm As New GameMove(x, y, currentplayer)

                        Dim json As String = Newtonsoft.Json.JsonConvert.SerializeObject(gm, Newtonsoft.Json.Formatting.None)
                        Console.WriteLine(json)

                        Dim bytes = Encoding.Default.GetBytes(json)


                        Try
                            Dim responsebytes2 = webClient.UploadData("http://localhost:8000/gamemove", "POST", bytes)
                            Dim responsebody2 = (New Text.UTF8Encoding).GetString(responsebytes2)

                            Console.WriteLine(responsebody2)
                            'If responsebody.IndexOf("Move Complete") >= 0 Then

                            'End If
                            If responsebody2.IndexOf("Game Over") >= 0 Then
                                gameinprogress = False
                            End If
                        Catch ex As Exception
                            Console.WriteLine(ex.Message)
                        End Try
                    ElseIf gs IsNot Nothing AndAlso (gs.PlayerWin > 0 Or gs.PlayerWin = -1) Then
                        'availablemoves.Clear()
                        'For i As Integer = 1 To 9
                        '    availablemoves.Add(i)
                        'Next
                        Console.WriteLine("Wait for game to Start...")
                    End If

                End If
            Catch ex As Exception
                Console.WriteLine(ex.Message)
            End Try


            Threading.Thread.Sleep(100)
        Loop
    End Sub
End Module
