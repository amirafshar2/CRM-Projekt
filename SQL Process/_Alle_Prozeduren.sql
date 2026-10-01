-- Alle gespeicherten Prozeduren für DBCRM in einem Skript
-- Ausführen NACH "Update-Database"
USE DBCRM;
GO

-- ===== Aktivitydetale.sql =====
CREATE PROCEDURE Aktivitydetale
	@search int 
AS
BEGIN
	SELECT        dbo.ACTİVİTY.id, dbo.CUSTOMERs.Company AS [Muşteri Firması], dbo.ACTİVİTY.User_id AS GörevliKodu, dbo.ACTİVİTY.İnfo AS Açıklama, dbo.ACTİVİTY.RegDate AS [Kayıt tarihi], dbo.CUSTOMERs.Name AS [Müşteri isim], 
                         dbo.CUSTOMERs.Phone AS [Tel No], dbo.CUSTOMERs.Email AS email
FROM            dbo.ACTİVİTY INNER JOIN
                         dbo.ACTİVİTY_CATEGORY ON dbo.ACTİVİTY.ActivityCategory_id = dbo.ACTİVİTY_CATEGORY.id INNER JOIN
                         dbo.CUSTOMERs ON dbo.ACTİVİTY.Customer_id = dbo.CUSTOMERs.id
WHERE        (dbo.ACTİVİTY.DeletStatus = 0) AND (dbo.ACTİVİTY.id = @Search)
END
GO

-- ===== Custumer_Bakiye_Search.sql =====
-- ================================================
-- Template generated from Template Explorer using:
-- Create Procedure (New Menu).SQL
--
-- Use the Specify Values for Template Parameters 
-- command (Ctrl-Shift-M) to fill in the parameter 
-- values below.
--
-- This block of comments will not be included in
-- the definition of the procedure.
-- ================================================
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
-- =============================================
-- Author:		<Author,,Name>
-- Create date: <Create Date,,>
-- Description:	<Description,,>
-- =============================================
CREATE PROCEDURE Custumer_Bakiye_Search
@Search int
AS
BEGIN
	SELECT        dbo.CUSTOMERs.id AS c_id, dbo.İNVOİCE.id, dbo.İNVOİCE.invoiceNumber AS [Rechnungs-Nr.], dbo.İNVOİCE.RegDate AS [Datum], dbo.İNVOİCE.TotalPrice AS [Gesamt],
                         ISNULL(dbo.İNVOİCE.ÖdemeTurarı, 0) AS [Bezahlt], ISNULL(dbo.İNVOİCE.Bakiye, dbo.İNVOİCE.TotalPrice) AS [Offen], dbo.İNVOİCE.ÖdemeDate AS [Letzte Zahlung],
                         CASE dbo.İNVOİCE.ödemeŞekli WHEN 0 THEN 'Bar' WHEN 1 THEN 'Kreditkarte' WHEN 2 THEN 'Kreditkarte (Raten)' WHEN 3 THEN 'Auf Ziel' WHEN 4 THEN 'Scheck' ELSE '' END AS [Zahlungsart],
                         dbo.İNVOİCE.VadeDate AS [Fällig am]
FROM            dbo.İNVOİCE INNER JOIN
                         dbo.CUSTOMERs ON dbo.İNVOİCE.Customer_id = dbo.CUSTOMERs.id
WHERE        (dbo.CUSTOMERs.id = @Search) AND (dbo.İNVOİCE.Deletestatus = 0)
ORDER BY dbo.İNVOİCE.id DESC
END
GO

-- ===== GetİnvoiceProduct.sql =====
-- ================================================
-- Template generated from Template Explorer using:
-- Create Procedure (New Menu).SQL
--
-- Use the Specify Values for Template Parameters 
-- command (Ctrl-Shift-M) to fill in the parameter 
-- values below.
--
-- This block of comments will not be included in
-- the definition of the procedure.
-- ================================================
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
-- =============================================
-- Author:		<Author,,Name>
-- Create date: <Create Date,,>
-- Description:	<Description,,>
-- =============================================
CREATE PROCEDURE GetİnvoiceProduct
@Search int
AS
BEGIN
	SELECT        dbo.İNVOİCE.id AS iid, dbo.İNVOİCE.invoiceNumber, dbo.İNVOİCE.RegDate, dbo.İNVOİCE.CeackOutDate, dbo.İNVOİCE.İsCheckedOut, dbo.İNVOİCE.TotalPrice, dbo.İNVOİCE.ÖdemeTurarı, dbo.İNVOİCE.ÖdemeDate, 
                         dbo.İNVOİCE.ödemeŞekli, dbo.İNVOİCE.VadeDate, dbo.İNVOİCE.Bakiye, dbo.İNVOİCE.Deletestatus, dbo.PRODUCTs.id, dbo.PRODUCTs.Name, dbo.PRODUCTs.Cap, dbo.PRODUCTs.Boy, dbo.PRODUCTs.Packing, 
                         dbo.PRODUCTs.Quality, dbo.PRODUCTs.Feature, dbo.PRODUCTs.Stock, dbo.PRODUCTs.Price, dbo.PRODUCTs.Kaplama, dbo.PRODUCTs.SaledPices, dbo.PRODUCTs.DINnumber, dbo.PRODUCTs.BrandName, 
                         dbo.PRODUCTs.Product_cod, dbo.PRODUCTs.picture, dbo.PRODUCTs.DeletStatus, dbo.PRODUCTs.Category, dbo.İNVOİCE.User_id, dbo.İNVOİCE.Customer_id
FROM            dbo.İNVOİCE INNER JOIN
                         dbo.PRODUCTs ON dbo.İNVOİCE.id = dbo.PRODUCTs.id
WHERE        ((dbo.İNVOİCE.Deletestatus = 0) AND (dbo.PRODUCTs.Stock = 0)) AND (dbo.İNVOİCE.id = @Search)
END
GO

-- ===== ReminderSearch.sql =====
-- ================================================
-- Template generated from Template Explorer using:
-- Create Procedure (New Menu).SQL
--
-- Use the Specify Values for Template Parameters 
-- command (Ctrl-Shift-M) to fill in the parameter 
-- values below.
--
-- This block of comments will not be included in
-- the definition of the procedure.
-- ================================================
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
-- =============================================
-- Author:		<Author,,Name>
-- Create date: <Create Date,,>
-- Description:	<Description,,>
-- =============================================
CREATE PROCEDURE ReminderSearch
	@Search nvarchar(max)
AS
BEGIN
	SELECT        dbo.REMİNDER.id, dbo.REMİNDER.Title AS [Hatırlatma Konusu], dbo.REMİNDER.Reminderİnfo AS [Hatırlatıcı Açıklaması], dbo.REMİNDER.ReminDate AS [Hatırlatma Tarihi], dbo.USERs.UserName AS Görevli
FROM            dbo.REMİNDER INNER JOIN
                         dbo.USERs ON dbo.REMİNDER.Users_id = dbo.USERs.id
WHERE      ((dbo.REMİNDER.İsDone = 0) AND (dbo.REMİNDER.DeletStatus = 0)) and ((dbo.USERs.Name like '%'+@Search+'%') or (dbo.USERs.UserName like '%'+@Search+'%') or (dbo.REMİNDER.Title like '%'+@Search+'%'))
END
GO

-- ===== SearchActivity.sql =====
-- ================================================
-- Template generated from Template Explorer using:
-- Create Procedure (New Menu).SQL
--
-- Use the Specify Values for Template Parameters 
-- command (Ctrl-Shift-M) to fill in the parameter 
-- values below.
--
-- This block of comments will not be included in
-- the definition of the procedure.
-- ================================================
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
-- =============================================
-- Author:		<Author,,Name>
-- Create date: <Create Date,,>
-- Description:	<Description,,>
-- =============================================
CREATE PROCEDURE SearchActivity
@Search nvarchar(max)
AS
BEGIN
	SELECT        dbo.ACTİVİTY.id, dbo.ACTİVİTY.Title AS Konu, dbo.ACTİVİTY.İnfo AS Açıklama, dbo.CUSTOMERs.Company AS Firma, dbo.ACTİVİTY.RegDate AS [Kayıt tarihi], dbo.ACTİVİTY.User_id AS GörevliKodu, dbo.USERs.UserName AS Temsilci
FROM            dbo.ACTİVİTY LEFT JOIN
                         dbo.CUSTOMERs ON dbo.ACTİVİTY.Customer_id = dbo.CUSTOMERs.id INNER JOIN
                         dbo.USERs ON dbo.ACTİVİTY.User_id = dbo.USERs.id
WHERE        (dbo.ACTİVİTY.DeletStatus = 0) AND ((dbo.CUSTOMERs.Name LIKE '%' + @Search + '%') OR
                         (dbo.CUSTOMERs.Company LIKE '%' + @Search + '%') OR
                         (dbo.USERs.UserName LIKE '%' + @Search + '%') OR
                         (dbo.ACTİVİTY.İnfo LIKE '%' + @Search + '%') OR
                         (dbo.ACTİVİTY.Title LIKE '%' + @Search + '%'))
END
GO

-- ===== SearchCustumer.sql =====
-- ================================================
-- Template generated from Template Explorer using:
-- Create Procedure (New Menu).SQL
--
-- Use the Specify Values for Template Parameters 
-- command (Ctrl-Shift-M) to fill in the parameter 
-- values below.
--
-- This block of comments will not be included in
-- the definition of the procedure.
-- ================================================
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
-- =============================================
-- Author:		<Author,,Name>
-- Create date: <Create Date,,>
-- Description:	<Description,,>
-- =============================================
CREATE PROCEDURE SearchCustumer
@Search nvarchar(max)
AS
BEGIN
	SELECT        dbo.CUSTOMERs.id, dbo.CUSTOMERs.Name AS İsim, dbo.CUSTOMERs.Company AS Firma, dbo.CUSTOMERs.Phone AS [Telefon NO], dbo.CUSTOMERs.Email AS [E-mail Adresi], dbo.CUSTOMERs.Regdate AS [Kayıt Tarihi], 
                         dbo.CUSTOMERs.Alacak AS [Bezahlt], dbo.CUSTOMERs.Bakiye AS [Umsatz], dbo.USERs.Name AS [Müştri Temsilcisi]
FROM            dbo.CUSTOMERs INNER JOIN
                         dbo.USERs ON dbo.CUSTOMERs.User_id = dbo.USERs.id
WHERE        (dbo.CUSTOMERs.DeletStatus = 0) AND ((dbo.CUSTOMERs.Name LIKE '%' + @Search + '%') OR
                         (dbo.CUSTOMERs.Company LIKE '%' + @Search + '%') OR
                         (dbo.CUSTOMERs.Phone LIKE '%' + @Search + '%') OR
                         (dbo.CUSTOMERs.Email LIKE '%' + @Search + '%') OR
                         (dbo.USERs.Name LIKE '%' + @Search + '%'))
END
GO

-- ===== SearchProduct.sql =====
USE [DBCRM]
GO
/****** Object:  StoredProcedure [dbo].[SearchProduct]    Script Date: 22.11.2023 17:11:11 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

create PROCEDURE SearchProduct
	@Search nvarchar(max)
AS
BEGIN
SELECT        id, Category AS Ürün, Name AS [Ürün Adı], Cap AS Çap, Boy, Quality AS Kalite, Kaplama, DINnumber AS DIN, Stock AS Stok, Price AS Fiyat, BrandName AS Marka, Packing AS Paket, picture AS Görsel,  
                         Feature AS Özellik
FROM            dbo.PRODUCTs
WHERE        ((DeletStatus = 0) AND (SaledPices =0)) AND  ((Name like '%'+ @Search +'%') or (Category like '%'+ @Search +'%') or (Kaplama like '%'+ @Search +'%') or (Cap like  '%'+ @Search +'%') or (Boy like  '%'+ @Search +'%') or (Quality like  '%'+ @Search +'%') or (DINnumber like  '%'+ @Search +'%') or (BrandName like  '%'+ @Search +'%'))
END
GO

-- ===== Searchinvoice.sql =====
-- ================================================
-- Template generated from Template Explorer using:
-- Create Procedure (New Menu).SQL
--
-- Use the Specify Values for Template Parameters 
-- command (Ctrl-Shift-M) to fill in the parameter 
-- values below.
--
-- This block of comments will not be included in
-- the definition of the procedure.
-- ================================================
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
-- =============================================
-- Author:		<Author,,Name>
-- Create date: <Create Date,,>
-- Description:	<Description,,>
-- =============================================
CREATE PROCEDURE Searchinvoice
@Search nvarchar(max)
AS
BEGIN
	SELECT        dbo.İNVOİCE.id, dbo.İNVOİCE.invoiceNumber AS [Fiş Numarası], dbo.İNVOİCE.RegDate AS [Sipariş Tarihi], dbo.İNVOİCE.TotalPrice AS [Toplam Tutar], dbo.İNVOİCE.ÖdemeTurarı, dbo.İNVOİCE.ödemeŞekli, 
                         dbo.İNVOİCE.ÖdemeDate AS [Ödeme Tarihi], dbo.İNVOİCE.Bakiye AS [bu Faturadan Kalan Bakiye], dbo.CUSTOMERs.Company AS Firma, dbo.CUSTOMERs.Name AS [Yetkili İsmi], 
                         dbo.USERs.UserName AS [Müşteri Temsilcisi]
FROM            dbo.İNVOİCE INNER JOIN
                         dbo.CUSTOMERs ON dbo.İNVOİCE.Customer_id = dbo.CUSTOMERs.id INNER JOIN
                         dbo.USERs ON dbo.İNVOİCE.User_id = dbo.USERs.id
WHERE        (dbo.İNVOİCE.Deletestatus = 0) and 
                        ((dbo.İNVOİCE.invoiceNumber LIKE '%' + @Search + '%') OR
                         (dbo.CUSTOMERs.Company LIKE '%' + @Search + '%') OR
                         (dbo.CUSTOMERs.Name LIKE '%' + @Search + '%') OR
                         (dbo.USERs.UserName LIKE '%' + @Search + '%'))
END
GO

-- ===== customer_user_id_read.sql =====
-- ================================================
-- Template generated from Template Explorer using:
-- Create Procedure (New Menu).SQL
--
-- Use the Specify Values for Template Parameters 
-- command (Ctrl-Shift-M) to fill in the parameter 
-- values below.
--
-- This block of comments will not be included in
-- the definition of the procedure.
-- ================================================
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
-- =============================================
-- Author:		<Author,,Name>
-- Create date: <Create Date,,>
-- Description:	<Description,,>
-- =============================================
CREATE PROCEDURE customer_user_id_read
@Search int
AS
BEGIN
	SELECT        dbo.USERs.id, dbo.CUSTOMERs.id AS Expr1
FROM            dbo.CUSTOMERs INNER JOIN
                         dbo.İNVOİCE ON dbo.CUSTOMERs.id = dbo.İNVOİCE.Customer_id INNER JOIN
                         dbo.USERs ON dbo.CUSTOMERs.User_id = dbo.USERs.id AND dbo.İNVOİCE.User_id = dbo.USERs.id
WHERE        (dbo.İNVOİCE.Deletestatus = 0) AND (dbo.CUSTOMERs.id = @Search)
END
GO

-- ===== İnvoice_Customer_Search.sql =====
-- ================================================
-- Template generated from Template Explorer using:
-- Create Procedure (New Menu).SQL
--
-- Use the Specify Values for Template Parameters 
-- command (Ctrl-Shift-M) to fill in the parameter 
-- values below.
--
-- This block of comments will not be included in
-- the definition of the procedure.
-- ================================================
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
-- =============================================
-- Author:		<Author,,Name>
-- Create date: <Create Date,,>
-- Description:	<Description,,>
-- =============================================
CREATE PROCEDURE İnvoice_Customer_Search
@Search nvarchar(max)
AS
BEGIN
SELECT        id, Company AS Firma, Name AS Yetkili, Adress AS Adres
FROM            dbo.CUSTOMERs
WHERE        (DeletStatus = 0) AND ((Company LIKE '%' + @Search + '%') OR (Name LIKE '%' + @Search + '%') OR   (Adress LIKE '%' + @Search + '%'))
END
GO
