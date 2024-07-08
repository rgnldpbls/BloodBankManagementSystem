CREATE TRIGGER trg_AcceptBloodDonation
ON BloodDonate
AFTER UPDATE
AS
BEGIN
	SET NOCOUNT ON;
	IF EXISTS (SELECT 1 FROM inserted WHERE Status = 'Accepted' AND Status != (SELECT Status FROM deleted))
	BEGIN
		DECLARE @BloodType NVARCHAR, @UnitNo INT, @DonateDate DATE, @ExpiryDate DATE;
		SELECT @BloodType = BloodType, @UnitNo = UnitNo, @DonateDate = DonateDate FROM inserted;
		SET @ExpiryDate = DATEADD(DAY, 42, @DonateDate);
		IF EXISTS (SELECT 1 FROM Inventory WHERE BloodType = @BloodType AND ExpiryDate = @ExpiryDate)
		BEGIN
			UPDATE Inventory
			SET Quantity = Quantity + @UnitNo, LastUpdated = GETDATE()
			WHERE BloodType = @BloodType AND ExpiryDate = @ExpiryDate;
		END
		ELSE
		BEGIN
			INSERT INTO Inventory(BloodType, Quantity, ExpiryDate, LastUpdated)
			VALUES (@BloodType, @UnitNo, @ExpiryDate, GETDATE());
		END
	END
END;

CREATE TRIGGER trg_AcceptBloodRequest
ON BloodRequest
AFTER UPDATE
AS
BEGIN
	SET NOCOUNT ON;
	IF EXISTS (SELECT 1 FROM inserted WHERE Status = 'Accepted' AND Status != (SELECT Status FROM deleted))
	BEGIN
		DECLARE @BloodType NVARCHAR, @UnitNo INT;
		DECLARE @RemainingQuantity INT;
		DECLARE @InventoryId INT, @InventoryQuantity INT;
		
		SELECT @BloodType = BloodType, @UnitNo = UnitNo FROM inserted;
		SET @RemainingQuantity = @UnitNo;
		
		DECLARE inventory_cursor CURSOR FOR
		SELECT Id, Quantity FROM Inventory WHERE BloodType = @BloodType AND ExpiryDate >= GETDATE()
		ORDER BY ExpiryDate;

		OPEN inventory_cursor;
		FETCH NEXT FROM inventory_cursor INTO @InventoryId, @InventoryQuantity;
		WHILE @@FETCH_STATUS = 0 AND @RemainingQuantity > 0
		BEGIN
			IF @InventoryQuantity >= @RemainingQuantity
			BEGIN
				UPDATE Inventory
				SET Quantity = Quantity - @RemainingQuantity, LastUpdated = GETDATE()
				WHERE Id = @InventoryId;
				SET @RemainingQuantity = 0;
			END
			ELSE
			BEGIN
				SET @RemainingQuantity = @RemainingQuantity - @InventoryQuantity;
				UPDATE Inventory
				SET Quantity = 0, LastUpdated = GETDATE()
				WHERE Id = @InventoryId;
			END

			FETCH NEXT FROM inventory_cursor INTO @InventoryId, @InventoryQuantity;
		END

		CLOSE inventory_cursor;
		DEALLOCATE inventory_cursor;

		IF @RemainingQuantity > 0
		BEGIN
			RAISERROR('Not enough blood available to fulfill the request.', 16, 1);
			ROLLBACK TRANSACTION;
		END
	END
END;