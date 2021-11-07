Imports System.Data
Imports System.Data.SqlClient
Imports siteInclude
Imports System.Net.Mail
Imports System.Net.Mail.SmtpClient

Partial Class shop_receipt
    Inherits BasePage
    Protected Const _gvBasket_productPos = 0
    Protected Const _gvBasket_rowPricePos = 3
    Protected currencySign As String = ""
    Private Const _debug As Boolean = True

    Protected Sub Page_Load(ByVal sender As Object, ByVal e As EventArgs) Handles Me.Load
        'lblPrefix.Text = Context.Items("orderPrefix")
        'lblOrderID.Text = Context.Items("orderID")
        'lblPrefix.Text = Session("EBTmpOrderPrefix")
        lblOrderID.Text = Session("EBTmpOrderID") & UCase(Session("EBShopCountry"))
        gvBasket.DataSource = Profile.EBCart.Items
        gvBasket.DataBind()
        If Session("EBTmpOrderPrefix") = "25" Then
            If LCase(Session("EBTmpOrderType")) = "ideal" Then
                'iDeal
                lblComplete.Text = CType(GetLocalResourceObject("lblPaymentComplete4"), String)
                Select Case LCase(Session("EBTmpIDealStatus"))
                    Case "accepted"
                        lblPaymentResults.Text = CType(getDBResourceString("lblPaymentResults1"), String) & Session("EBTmpIDealTx")
                    Case "failure"
                        lblPaymentResults.Text = CType(getDBResourceString("lblPaymentResults3"), String)
                    Case Else
                        lblPaymentResults.Text = CType(getDBResourceString("lblPaymentResults4"), String)
                End Select
                'lblComplete.Text = CType(GetLocalResourceObject("lblPaymentComplete4"), String)
                lblComplete2.Text = getDBResourceString("lblComplete2")

            Else
                'Postal order
                'lblComplete.Text = CType(GetLocalResourceObject("lblPaymentComplete3"), String)
                lblComplete3.Text = getDBResourceString("lblComplete3")
            End If
        Else
            'Creditcard order
            'Live auth removed, dont show result
            If False Then
                If CType(Session("EBTmpOrderPaid"), String) <> "" Then
                    'Payment has already been taken, show results
                    Dim oConn As New SqlConnection(ConfigurationManager.ConnectionStrings("connectionString").ConnectionString)
                    Dim oCmd As New SqlCommand("procProtxByNewOrderIDSelect", oConn)
                    Dim da As New SqlDataAdapter
                    Dim ds As New DataSet
                    With oCmd
                        .CommandType = CommandType.StoredProcedure
                        .Parameters.Add(New SqlParameter("@newOrderID", SqlDbType.Int))
                        .Parameters.Add(New SqlParameter("@countryCode", SqlDbType.VarChar, 5))
                        .Parameters("@newOrderID").Value = Session("EBTmpOrderID")
                        .Parameters("@countryCode").Value = Session("EBShopCountry")
                    End With
                    Try
                        If oCmd.Connection.State = 0 Then oCmd.Connection.Open()
                        da = New SqlDataAdapter(oCmd)
                        da.Fill(ds)
                        If ds.Tables(0).Rows.Count > 0 Then
                            If LCase(ds.Tables(0).Rows(0)("status")) = "ok" Then
                                lblPaymentResults.Text = CType(getDBResourceString("lblPaymentResults1"), String) & " " & ds.Tables(0).Rows(0)("vendorTxCode") & "<br>"
                                'lblPaymentResults.Text = "Payment complete.<br>Transaction number " & ds.Tables(0).Rows(0)("vendorTxCode") & "<br>"
                                lblPaymentResults.Text = lblPaymentResults.Text & CType(getDBResourceString("lblPaymentResults2"), String)
                                'lblPaymentResults.Text = lblPaymentResults.Text & "The voucher has been emailed to the recipient, and a copy has been sent to your email address. <font color='red'><b> STILL TO BE DONE!</b></font>"
                                lblComplete.Text = CType(getDBResourceString("lblPaymentComplete2"), String)
                            Else
                                'Payment failed
                                'If CBool(Session("EBTmpOrderPaid")) Then
                                'lblPaymentResults.Text = CType(GetLocalResourceObject("lblPaymentResults1"), String)
                                'lblComplete.Text = CType(GetLocalResourceObject("lblPaymentComplete1"), String)
                                'End If
                                lblPaymentResults.Text = CType(getDBResourceString("lblPaymentResults3"), String)
                                lblComplete.Text = CType(getDBResourceString("lblPaymentComplete1"), String)
                            End If
                        End If
                    Catch ex As Exception
                        Response.Write(ex)
                        Response.End()
                    End Try
                End If
            End If
            lblComplete4.Text = getDBResourceString("lblComplete4")
        End If
        If Session("EBTmpVoucherNumber") <> "" Then
            'Show voucher numbers if orderwas voucher only
            lblCompleteVoucher.Text = getDBResourceString("lblCompleteVoucher") & " <b>" & Session("EBTmpVoucherNumber") & "</b>"
        End If
        loadDBResources()
    End Sub
    Protected Sub Page_Unload(ByVal sender As Object, ByVal e As EventArgs) Handles Me.Unload
        If Not _debug Then
            'Empty basket
            Profile.EBCart.emptyBasket()
            'Clear tmp session data
            Session("EBTmpOrderPrefix") = ""
            Session("EBTmpOrderID") = ""
            Session("EBTmpOrderType") = ""
            Session("EBTmpOrderPaid") = ""
            Session("EBTmpUniqueID") = ""
            Session("EBTmpIDealStatus") = ""
            Session("EBTmpIDealTx") = ""
            Session("EBAffClickThroughID") = ""
            Session("EBTmpVoucherNumber") = ""
            'Remove basket from database
            removeCart(Session("EBsID"))
            Session("EBsID") = ""
        End If
    End Sub
    Protected Sub gvBasket_Load(ByVal sender As Object, ByVal e As EventArgs)
        Try
            currencySign = Profile.EBCart.CurrencySign
            'Dim lblTotal As Label = gvBasket.FooterRow.FindControl("lblTotal")
            'lblTotal.ForeColor = Drawing.Color.Black
            'gvBasket.FooterRow.Cells(_gvBasket_productPos).Text = CType(GetLocalResourceObject("_cbLblShipping"), String) & "<br>"
            'gvBasket.FooterRow.Cells(_gvBasket_productPos).Text = gvBasket.FooterRow.Cells(_gvBasket_productPos).Text & CType(GetLocalResourceObject("_cbLblVAT"), String) & "<br>"
            'gvBasket.FooterRow.Cells(_gvBasket_productPos).Text = gvBasket.FooterRow.Cells(_gvBasket_productPos).Text & "<b>" & CType(GetLocalResourceObject("_cbLblTotal"), String) & "</b>"
            'gvBasket.FooterRow.Cells(_gvBasket_rowPricePos).Text = "0.00<br>" & Profile.EBCart.TotalInc - Profile.EBCart.TotalEx & "<br><b>" & Profile.EBCart.TotalInc & "</b>"
            Dim lblShippingCostEx As Label = gvBasket.FooterRow.FindControl("lblShippingCostEx")
            Dim lblShippingCostInc As Label = gvBasket.FooterRow.FindControl("lblShippingCostInc")
            Dim lblVatCost As Label = gvBasket.FooterRow.FindControl("lblVatCost")
            Dim lblTotalCost As Label = gvBasket.FooterRow.FindControl("lblTotalCost")
            Dim lblVoucherDiscountText As Label = gvBasket.FooterRow.FindControl("lblVoucherDiscountText")
            Dim lblCouponDiscountText As Label = gvBasket.FooterRow.FindControl("lblCouponDiscountText")
            Dim lblVoucherDiscount As Label = gvBasket.FooterRow.FindControl("lblVoucherDiscount")
            Dim lblCouponDiscount As Label = gvBasket.FooterRow.FindControl("lblCouponDiscount")
            Dim lblTmp As New Label
            Dim VoucherDiscount As Decimal = 0
            If Profile.EBCart.VoucherNumber <> "" Then VoucherDiscount = Profile.EBCart.VoucherCredit
            'setShipping(lblShippingCostEx, lblShippingCostInc, lblTmp)
            lblShippingCostEx.Text = currencySign & Profile.EBCart.Shipping
            lblShippingCostInc.Text = currencySign & Profile.EBCart.ShippingTotal
            lblVatCost.Text = currencySign & FormatNumber(CStr((Profile.EBCart.GoodsVat) + (CDec(Profile.EBCart.ShippingTotal) - CDec(Profile.EBCart.Shipping))), 2)
            lblTotalCost.Text = currencySign & Profile.EBCart.TotalInc
            If VoucherDiscount <> 0 Then
                'Show discount rows
                If Profile.EBCart.VoucherIsCoupon Then
                    lblCouponDiscountText.Visible = True
                    lblCouponDiscount.Text = currencySign & VoucherDiscount & "<br>"
                Else
                    lblVoucherDiscountText.Visible = True
                    lblVoucherDiscount.Text = currencySign & VoucherDiscount & "<br>"
                End If
            End If
        Catch ex As Exception
            'Session must have timed out
            lblPaymentResults.Text = CType(GetLocalResourceObject("errSessionTimeoutMsg"), String)
            lblReceipt.Text = CType(GetLocalResourceObject("errSessionTimeout"), String)
            lblOrderNo.Visible = False
            lblDash.Visible = False
        End Try
    End Sub
    Protected Sub addShippingToTotal()
        Dim lblTotalIncVat As Label = gvBasket.FooterRow.FindControl("lblTotalIncVat")
        Dim lblShippingTotalCost As Label = gvBasket.FooterRow.FindControl("lblShippingTotalCost")
        Try
            lblTotalIncVat.Text = CStr(CDec(lblTotalIncVat.Text) + CDec(lblShippingTotalCost.Text))
        Catch ex As Exception
            lblTotalIncVat.Text = "Err#"
        End Try
    End Sub
    Protected Sub setShipping(ByRef lblShippingCost As Label, ByRef lblShippingTotalCost As Label, ByRef lblShippingVatRate As Label)
        Dim oConn As New SqlConnection(ConfigurationManager.ConnectionStrings("connectionString").ConnectionString)
        Dim oCmd As New SqlCommand("procShippingWeightByCountryWeightSelect", oConn)
        Dim da As New SqlDataAdapter
        Dim ds As New DataSet
        Dim cost As String = "0.00"
        Dim vatRate As String = "0.0"
        Dim totalCost As String = "0.00"
        With oCmd
            .CommandType = CommandType.StoredProcedure
            .Parameters.Add(New SqlParameter("@countryCode", SqlDbType.VarChar, 5))
            .Parameters.Add(New SqlParameter("@weight", SqlDbType.Int))
            .Parameters("@countryCode").Value = Session("EBLanguage")
            .Parameters("@weight").Value = Profile.EBCart.TotalWeight
        End With
        Try
            If oCmd.Connection.State = 0 Then oCmd.Connection.Open()
            da = New SqlDataAdapter(oCmd)
            da.Fill(ds)
            If ds.Tables(0).Rows.Count > 0 Then
                cost = FormatNumber(ds.Tables(0).Rows(0)("price"), 2)
                vatRate = FormatNumber(ds.Tables(0).Rows(0)("VatRate"), 1)
                totalCost = FormatNumber(ds.Tables(0).Rows(0)("price") + ds.Tables(0).Rows(0)("shippingVatCost"), 2)
            End If
        Catch ex As Exception
            Response.Write(ex)
        Finally
            ds.Dispose()
            da.Dispose()
            oCmd.Dispose()
            oConn.Dispose()
        End Try
        lblShippingCost.Text = cost
        lblShippingVatRate.Text = vatRate
        lblShippingTotalCost.Text = totalCost
    End Sub
    Protected Sub gvBasket_dataBound(ByVal sender As Object, ByVal e As EventArgs)
        'Check for a consultancy voucher in the basket (Its product code will start with 'VC'
        For Each row As GridViewRow In gvBasket.Rows
            Dim lblProdCode As Label
            If row.RowType = DataControlRowType.DataRow Then
                lblProdCode = row.FindControl("lblProductCode")
                If Len(lblProdCode.Text) > 2 Then
                    If LCase(Left(lblProdCode.Text, 2)) = "vc" Then
                        'Consulatncy voucher found, create it in database and show voucher number to the user onscreen
                        Dim replys As String = Mid(lblProdCode.Text, 3, 1)
                        createConsultancyVoucher(replys)
                    End If
                End If
            End If
        Next
    End Sub
    Protected Sub createConsultancyVoucher(ByVal replys As Integer)
        Dim oConn As New SqlConnection(ConfigurationManager.ConnectionStrings("connectionString").ConnectionString)
        Dim oCmd As New SqlCommand("procConsultancyCreateInsert", oConn)
        Dim cardName As String = ""
        Dim email As String = ""
        Dim id As Integer = 0
        Dim bError As Boolean = False
        Dim code As String = ""
        getNameAndEmail(cardName, email, id, Session("EBTmpOrderID"))
        With oCmd
            .CommandType = CommandType.StoredProcedure
            .Parameters.Add(New SqlParameter("@countryCode", SqlDbType.VarChar, 5))
            .Parameters.Add(New SqlParameter("@cardName", SqlDbType.VarChar, 50))
            .Parameters.Add(New SqlParameter("@orderID", SqlDbType.Int))
            .Parameters.Add(New SqlParameter("@replys", SqlDbType.Int))
            .Parameters.Add(New SqlParameter("@email", SqlDbType.VarChar, 100))
            .Parameters.Add(New SqlParameter("@code", SqlDbType.VarChar, 10))
            .Parameters("@countryCode").Value = Session("EBLanguage")
            .Parameters("@cardName").Value = cardName
            .Parameters("@orderID").Value = id
            .Parameters("@replys").Value = replys
            .Parameters("@email").Value = email
            .Parameters("@code").Direction = ParameterDirection.Output
        End With
        Try
            If oCmd.Connection.State = 0 Then oCmd.Connection.Open()
            oCmd.ExecuteNonQuery()
            code = oCmd.Parameters("@code").Value
            lblConsultancy.Text = lblConsultancy.Text & "Your unique Consultancy Voucher number is " & code & ".<br>Details will be emailed to <font color='blue'>" & email & "</font>"
            lblConsultancy.Text = lblConsultancy.Text & "<br><br>Or you get get started right now by clicking <a href='../consultancy.aspx?code=" & code & "'>here</a>."
        Catch ex As Exception
            bError = True
            Try
                Dim si As New siteInclude
                si.addError("shop/receipt.aspx.vb", "createConsultancyVoucher(" & replys & "); " & ex.ToString & "<br>")
                si = Nothing
                lblConsultancy.Text = lblConsultancy.Text & "<font color='red'>An error occured whilest creating your consultancy voucher. Please contact <a href='mailto:support@emotionalbliss.com'>support@emotionalbliss.com</a> including your order number.<br>Please accept our appologies. We will resolve the problem as soon as possible."
            Catch ex1 As Exception
            End Try
        End Try
        If Not bError Then
            sendConfirmationEmail(cardName, email, code)
        End If
    End Sub
    Protected Sub getNameAndEmail(ByRef cn As String, ByRef e As String, ByRef orderID As Integer, ByVal newOrderID As Integer)
        Dim oConn As New SqlConnection(ConfigurationManager.ConnectionStrings("connectionString").ConnectionString)
        Dim ocmd As New SqlCommand("procShopOrderByNewIDSelect", oConn)
        Dim da As New SqlDataAdapter
        Dim ds As New DataSet

        With ocmd
            .CommandType = CommandType.StoredProcedure
            .Parameters.Add(New SqlParameter("@newOrderID", SqlDbType.Int))
            .Parameters.Add(New SqlParameter("@countryCode", SqlDbType.VarChar, 5))
            .Parameters("@newOrderID").Value = newOrderID
            .Parameters("@countryCode").Value = Session("EBLanguage")
        End With
        Try
            If ocmd.Connection.State = 0 Then ocmd.Connection.Open()
            da = New SqlDataAdapter(ocmd)
            da.Fill(ds)
            If ds.Tables(0).Rows.Count > 0 Then
                cn = ds.Tables(0).Rows(0)("billName")
                e = ds.Tables(0).Rows(0)("email")
                orderID = ds.Tables(0).Rows(0)("id")
            End If
        Catch ex As Exception
            Try
                Dim si As New siteInclude
                si.addError("shop/receipt.aspx.vb", "getNameAndEmail(" & orderID & "); " & ex.ToString)
                si = Nothing
                lblConsultancy.Text = lblConsultancy.Text & "<font color='red'>An error occured whilest creating your consultancy voucher. Please contact <a href='mailto:support@emotionalbliss.com'>support@emotionalbliss.com</a> including your order number.<br>Please accept our appologies. We will resolve the problem as soon as possible.<br>"
            Catch ex1 As Exception
            End Try
        Finally
            ds.Dispose()
            da.Dispose()
            ocmd.Dispose()
            oConn.Dispose()
        End Try
    End Sub
    Protected Sub sendConfirmationEmail(ByVal cardname As String, ByVal email As String, ByVal voucherNo As String)
        If voucherNo <> "" Then
            Dim toAdd As String = email
            Dim ccAdd As String = ""
            Dim subject As String = "EB Consultacy"
            Dim msg As MailMessage
            Dim chk As CheckBox
            Dim plainView As AlternateView
            Dim htmlView As AlternateView
            Dim emailBody As String = "This is an automated email from EmotionalBliss." & Chr(13) & Chr(13)
            emailBody = emailBody & "Your unique Consultancy ID is " & voucherNo & Chr(13) & Chr(13)
            emailBody = emailBody & "Please visit <a href='http://www.emotionalbliss.co.uk/consultancy.aspx'>http://www.emotionalbliss.co.uk/consultancy.aspx</a> to begin your sessions." & Chr(13) & Chr(13) & Chr(13)
            emailBody = emailBody & "Emotional Bliss"
            msg = New MailMessage
            msg.To.Add(toAdd)
            If ccAdd <> "" Then msg.CC.Add(ccAdd)
            msg.From = New MailAddress("noreply@emotionalbliss.com")
            msg.Subject = subject
            msg.IsBodyHtml = True
            plainView = AlternateView.CreateAlternateViewFromString(emailBody, Nothing, "text/plain")
            htmlView = AlternateView.CreateAlternateViewFromString(Replace(emailBody, Chr(13), "<br>"), Nothing, "text/html")
            msg.AlternateViews.Add(plainView)
            msg.AlternateViews.Add(htmlView)
            Dim client As New SmtpClient
            client.Send(msg)
            msg.Dispose()
        End If
    End Sub
    Protected Sub removeCart(ByVal sid As String)
        If sid <> "" Then
            Dim oConn As New SqlConnection(ConfigurationManager.ConnectionStrings("connectionString").ConnectionString)
            Dim oCmd As New SqlCommand("procEBCartByIDDelete", oConn)
            With oCmd
                .CommandType = CommandType.StoredProcedure
                .Parameters.Add(New SqlParameter("@id", SqlDbType.Int))
                .Parameters("@id").Value = sid
            End With
            Try
                If oCmd.Connection.State = 0 Then oCmd.Connection.Open()
                oCmd.ExecuteNonQuery()
            Catch ex As Exception
                Dim si As New siteInclude
                si.addError("shop/receipt.aspx.vb", "removeCart(" & sid & "); Could not be removed; " & ex.ToString)
                si = Nothing
            Finally
                oCmd.Dispose()
                oConn.Dispose()
            End Try
        End If
    End Sub
    Protected Sub loadDBResources()
        lblReceipt.Text = getDBResourceString("lblReceipt")
        lblOrderNo.Text = getDBResourceString("lblOrderNo")
        If gvBasket.Rows.Count > 0 Then
            Dim lblVAT As Label = gvBasket.FooterRow.FindControl("lblVAT")
            Dim lblVoucherDiscountText As Label = gvBasket.FooterRow.FindControl("lblVoucherDiscountText")
            Dim lblTotal As Label = gvBasket.FooterRow.FindControl("lblTotal")
            Dim lblCouponDiscountText As Label = gvBasket.FooterRow.FindControl("lblCouponDiscountText")
            Dim lblShipping As Label = gvBasket.FooterRow.FindControl("lblShipping")
            lblVAT.Text = getDBResourceString("lblVAT")
            lblVoucherDiscountText.Text = getDBResourceString("lblVoucherDiscountText")
            lblTotal.Text = getDBResourceString("lblTotal")
            lblCouponDiscountText.Text = getDBResourceString("lblCouponDiscountText")
            lblShipping.Text = getDBResourceString("lblShipping")

        End If

    End Sub
End Class
