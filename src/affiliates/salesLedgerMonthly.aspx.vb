﻿Imports System.Data

Partial Class affiliates_salesLedgerMonthly
    Inherits BasePage
    Private Const _gvSalesLedger_chargedPos As Integer = 2
    Private Const _gvSalesLedger_creditPos As Integer = 4
    Private Const _gvSalesLedger_chequePos As Integer = 6
    Private Const _gvSalesLedger_chequeSpacerPos As Integer = 7
    Private Const _gvSalesLedger_affiliatePos As Integer = 8
    Private Const _gvSalesLedger_affiliateSpacerPos As Integer = 9
    Private Const _gvSalesLedger_affiliateAccPos As Integer = 10
    Private Const _gvSalesLedger_affiliateAccSpacerPos As Integer = 11
    Private Const _gvSalesLedger_vatPos As Integer = 12
    Private Const _gvSalesLedger_totalPos As Integer = 14
    Private Const _gvSalesLedgerDay_goodsPos As Integer = 2
    Private Const _gvSalesLedgerDay_vatPos As Integer = 4
    Private Const _gvSalesLedgerDay_affiliateSpacerPos As Integer = 7

    'System
    Protected Sub Page_Load(ByVal sender As Object, ByVal e As EventArgs) Handles Me.Load
        If Membership.GetUser Is Nothing Or Session("EBAffID") = "" Then Response.Redirect("default.aspx")
        If Not Page.IsPostBack Then
            lblCountryCode.Text = UCase(Session("EBAffEBDistributorCountryCode"))
            lblCurrency.Text = UCase(Session("EBAffCurrencyCode"))
        End If
        lblDateError.Text = ""
    End Sub
    Protected Sub Page_LoadComplete(ByVal sender As Object, ByVal e As EventArgs) Handles Me.LoadComplete
        lnkShowComplete.NavigateUrl = "salesLedgerpop.aspx?startdate=" & date1.getStartDate & "&enddate=" & date1.getEndDate & "&type=" & drpType.SelectedValue
    End Sub

    'Page
    Protected Sub drpType_dataBinding(ByVal sender As Object, ByVal e As EventArgs)
        drpType.Items.Clear()
        drpType.Items.Add(New ListItem("All", "%"))
        drpType.Items.Add(New ListItem("AMEX", "amex"))
    End Sub
    Protected Sub drpCountry_dataBound(ByVal sender As Object, ByVal e As EventArgs)
        'Rename the 'all' items to 'Please Choose....'
        drpCountry.Items(0).Text = "Please Choose...."
    End Sub

    'User
    Protected Sub gvSalesLedger_selectedIndexChanged(ByVal sender As Object, ByVal e As EventArgs)
        panLog.Visible = False
        panDetails.Visible = True
        lblDay.Text = "Sales ledger for " & FormatDateTime(gvSalesLedger.DataKeys(gvSalesLedger.SelectedIndex).Value, DateFormat.LongDate)
    End Sub
    Protected Function showDate(ByVal d As Object) As String
        Dim result As String = ""
        If Not IsDBNull(d) Then result = FormatDateTime(d.ToString, DateFormat.LongDate)
        Return result
    End Function
    Protected Sub lnkBack_click(ByVal sender As Object, ByVal e As EventArgs)
        panDetails.Visible = False
        panLog.Visible = True
    End Sub
    Protected Sub gvSalesLedger_databound(ByVal sender As Object, ByVal e As EventArgs)
        Dim di As Integer = 0
        If InStr(LCase(Request.ServerVariables("PATH_INFO")), "accounts") > 0 Then
            'New accounts page is being viewed, startdate must be >= 1st Oct 2009
            di = DateDiff(DateInterval.Day, CDate(FormatDateTime(date1.getStartDate, DateFormat.LongDate)), CDate(FormatDateTime("1 october 2009", DateFormat.LongDate)))
            If di < 0 Then
                'Invalid
                lblDateError.Text = "<font color='red'>The Start Date cannot be older than 1st October 2009. The search results will be incorrect.<br>If you would like to view transactions before 1st October 2009 then please use <a href='salesLedgerMonthly.aspx'>this page</a>.</font>"
            End If
        Else
            'Old accounts page is being viewed, end date must be before 1st Oct 2009
            di = DateDiff(DateInterval.Day, CDate(FormatDateTime("1 october 2009", DateFormat.LongDate)), CDate(FormatDateTime(date1.getEndDate, DateFormat.LongDate)))
            If di >= 0 Then
                'Invalid
                lblDateError.Text = "<font color='red'>The End Date cannot be after the 1st October 2009. The search results will be incorrect.<br>If you would like to view transactions after 1st October 2009 then please use <a href='salesLedgerAccounts.aspx'>this page</a>.</font>"
            End If
        End If
        Dim iCharged As Integer = 0
        Dim dCredit As Decimal = 0
        Dim dCheque As Decimal = 0
        Dim dAffiliate As Decimal = 0
        Dim dAffiliateAcc As Decimal = 0
        Dim dVat As Decimal = 0
        Dim dGoods As Decimal = 0
        Dim dTotal As Decimal = 0
        Dim lblTotal As Label
        Dim bShowFooter As Boolean = False
        Dim lblActionID As Label
        Dim lblGoods As Label
        Dim lnkDay As LinkButton
        For Each row As GridViewRow In gvSalesLedger.Rows
            If row.RowType = DataControlRowType.DataRow Then
                bShowFooter = True
                lblActionID = row.FindControl("lblActionID")
                lblTotal = row.FindControl("lblTotalIncVat")
                lblGoods = row.FindControl("lblGoodsTotal")
                lnkDay = row.FindControl("lnkDay")
                'Get the goods total (for sales only)
                If lblActionID.Text = "1" Then lblGoods.Text = getGoodsTotal(lnkDay.Text)
                'Make font red if row is a refund row
                If lblActionID.Text = "2" Or lblActionID.Text = "3" Then
                    For Each cell As Object In row.Cells
                        cell.forecolor = Drawing.Color.Red
                        lblTotal.ForeColor = Drawing.Color.Red
                    Next
                End If
                Try
                    iCharged = iCharged + row.Cells(_gvSalesLedger_chargedPos).Text
                    dCredit = dCredit + row.Cells(_gvSalesLedger_creditPos).Text
                    dGoods = dGoods + Convert.ToDecimal(lblGoods.Text)
                    If drpType.SelectedValue = "%" Then dCheque = dCheque + row.Cells(_gvSalesLedger_chequePos).Text
                    If drpType.SelectedValue = "%" Then dAffiliate = dAffiliate + row.Cells(_gvSalesLedger_affiliatePos).Text
                    If drpType.SelectedValue = "%" Then dAffiliateAcc = dAffiliateAcc + row.Cells(_gvSalesLedger_affiliateAccPos).Text
                    'lblError.Text = lblError.Text & row.Cells(_gvSalesLedger_vatPos).Text & ","
                    'dVat = dVat + row.Cells(_gvSalesLedger_vatPos).Text
                    dTotal = dTotal + lblTotal.Text
                Catch ex As Exception
                    lblError.Text = lblError.Text & ex.ToString
                End Try
            End If
        Next

        If bShowFooter Then
            gvSalesLedger.FooterRow.Cells(_gvSalesLedger_chargedPos).Text = iCharged
            gvSalesLedger.FooterRow.Cells(_gvSalesLedger_creditPos).Text = FormatNumber(dCredit, 2)
            If drpType.SelectedValue = "%" Then gvSalesLedger.FooterRow.Cells(_gvSalesLedger_chequePos).Text = FormatNumber(dCheque, 2)
            If drpType.SelectedValue = "%" Then gvSalesLedger.FooterRow.Cells(_gvSalesLedger_affiliatePos).Text = FormatNumber(dAffiliate, 2)
            If drpType.SelectedValue = "%" Then gvSalesLedger.FooterRow.Cells(_gvSalesLedger_affiliateAccPos).Text = FormatNumber(dAffiliateAcc, 2)
            'gvSalesLedger.FooterRow.Cells(_gvSalesLedger_vatPos).Text = FormatNumber(dVat, 2)
            gvSalesLedger.FooterRow.Cells(siteInclude.getGVRowByHeader(gvSalesLedger, "Goods Total")).Text = FormatNumber(dGoods, 2)
            gvSalesLedger.FooterRow.Cells(siteInclude.getGVRowByHeader(gvSalesLedger, "Total (Inc Vat)")).Text = FormatNumber(dTotal, 2)
        End If
    End Sub
    Protected Sub gvSalesLedgerDay_dataBound(ByVal sender As Object, ByVal e As EventArgs)
        Dim dGoods As Decimal = 0
        Dim dVat As Decimal = 0
        Dim bShowFooter As Boolean = False
        Dim lbl As Label
        For Each row As GridViewRow In gvSalesLedgerDay.Rows
            If row.RowType = DataControlRowType.DataRow Then
                bShowFooter = True
                dGoods = dGoods + row.Cells(_gvSalesLedgerDay_goodsPos).Text
                dVat = dVat + row.Cells(_gvSalesLedgerDay_vatPos).Text
                'Highlight affAccount orders in red
                lbl = row.FindControl("lblOrderType")
                If LCase(lbl.Text) = "affaccount" Then
                    'Aff Account order found, highlight row and show text
                    row.BackColor = Drawing.Color.Pink
                    row.Cells(_gvSalesLedgerDay_affiliateSpacerPos).Text = "Aff Account"
                End If

            End If
        Next
        If bShowFooter Then
            gvSalesLedgerDay.FooterRow.Cells(_gvSalesLedgerDay_goodsPos).Text = FormatNumber(dGoods, 2)
            gvSalesLedgerDay.FooterRow.Cells(_gvSalesLedgerDay_vatPos).Text = FormatNumber(dVat, 2)
        End If
    End Sub
    Protected Sub sqlSalesLedgerDay_selecting(ByVal sender As Object, ByVal e As SqlDataSourceSelectingEventArgs)
        'This will load the 'actionID' parameter with a 1 (Sale) or 2 (Refund), so that only the correct items are returned for the selcted day

        Dim lblActionID As Label = gvSalesLedger.SelectedRow.FindControl("lblActionID")
        'lblError.Text = lblActionID.Text & " " & Now().TimeOfDay.ToString
        e.Command.Parameters("@actionID").Value = lblActionID.Text
    End Sub
    Protected Sub drpType_selectedIndexChanged(ByVal sender As Object, ByVal e As EventArgs)
        'If '%'(All) is selected, then show the extra columns in the SalesLedger Gridview
        If drpType.SelectedValue = "%" Then
            gvSalesLedger.Columns(_gvSalesLedger_chequePos).Visible = True
            gvSalesLedger.Columns(_gvSalesLedger_chequeSpacerPos).Visible = True
            gvSalesLedger.Columns(_gvSalesLedger_affiliatePos).Visible = True
            gvSalesLedger.Columns(_gvSalesLedger_affiliateSpacerPos).Visible = True
        Else
            gvSalesLedger.Columns(_gvSalesLedger_chequePos).Visible = False
            gvSalesLedger.Columns(_gvSalesLedger_chequeSpacerPos).Visible = False
            gvSalesLedger.Columns(_gvSalesLedger_affiliatePos).Visible = False
            gvSalesLedger.Columns(_gvSalesLedger_affiliateSpacerPos).Visible = False
        End If
        gvSalesLedger.Columns(_gvSalesLedger_affiliateAccPos).Visible = gvSalesLedger.Columns(_gvSalesLedger_affiliatePos).Visible
        gvSalesLedger.Columns(_gvSalesLedger_affiliateAccSpacerPos).Visible = gvSalesLedger.Columns(_gvSalesLedger_affiliateSpacerPos).Visible
    End Sub
    Protected Function getGoodsTotal(ByVal d As String) As Decimal
        Dim result As Decimal = 0
        Dim dt As New DataTable
        Try
            Dim param() As String = {"@countryCode", "@day", "@type"}
            Dim paramValue() As String = {drpCountry.SelectedValue, d, drpType.SelectedValue}
            Dim paramType() As SqlDbType = {SqlDbType.VarChar, SqlDbType.VarChar, SqlDbType.VarChar}
            Dim paramSize() As Integer = {5, 20, 20}
            dt = siteInclude.getDataTable(param, paramValue, paramType, paramSize, "procShopOrderByScanDateGoodsTotalSelect")
            If dt.Rows.Count > 0 Then result = dt.Rows(0)("goods")
        Catch ex As Exception
            siteInclude.addError("maintenance/salesLedgerLog.aspx.vb", "getGoodsTotal(d=" & d & "); " & ex.ToString())
        Finally
            dt.Dispose()
        End Try
        Return result
    End Function
End Class
