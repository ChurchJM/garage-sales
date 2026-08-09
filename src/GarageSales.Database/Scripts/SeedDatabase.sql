SET NOCOUNT ON;
BEGIN TRANSACTION;

BEGIN TRY
    -------------------------------------------------------------------
    -- 1. SEED TABLE: ItemCategories
    -------------------------------------------------------------------
    PRINT 'Seeding Item Categories table...';
    
    SET IDENTITY_INSERT [dbo].[ItemCategories] ON; 

    MERGE INTO [dbo].[ItemCategories] WITH (HOLDLOCK) AS Target
    USING (VALUES 
        (1, N'Men''s Clothing', N'T-shirts, jackets, suits, jeans, workwear, coats'),
        (2, N'Women''s Clothing', N'Dresses, tops, outerwear, activewear, formal wear'),
        (3, N'Children''s & Baby Clothes', N'Onesies, toddler outfits, kids'' jackets, school clothes'),
        (4, N'Footwear', N'Sneakers, dress shoes, boots, sandals, cleats'),
        (5, N'Fashion Accessories', N'Handbags, belts, hats, scarves, costume jewelry, sunglasses'),
        (6, N'Baby Gear', N'Strollers, high chairs, baby carriers, pack-and-plays, monitors'),
        (7, N'Toys & Action Figures', N'Dolls, action figures, building blocks, playsets, stuffed animals'),
        (8, N'Board Games & Puzzles', N'Tabletop games, card games, jigsaw puzzles, lawn games'),
        (9, N'Video Games & Consoles', N'Retro consoles, hand-helds, game discs/cartridges, controllers'),
        (10, N'Consumer Electronics', N'TVs, soundbars, computer monitors, speakers, cables, chargers'),
        (11, N'Media (Movies & Music)', N'Vinyl records, CDs, DVDs, Blu-rays, cassette tapes'),
        (12, N'Books & Magazines', N'Fiction, non-fiction, textbooks, comic books, cookbooks, children''s books'),
        (13, N'Kitchen Small Appliances', N'Blenders, coffee makers, microwaves, air fryers, toaster ovens'),
        (14, N'Cookware & Dinnerware', N'Pots, pans, dish sets, glassware, silverware, bakeware'),
        (15, N'Home Decor & Wall Art', N'Framed prints, mirrors, vases, picture frames, candles, clocks'),
        (16, N'Indoor Furniture', N'Dressers, coffee tables, desks, bookshelves, side tables, dining chairs'),
        (17, N'Outdoor & Patio Furniture', N'Lawn chairs, patio tables, umbrellas, outdoor cushions, fire pits'),
        (18, N'Linens & Bedding', N'Comforters, sheet sets, pillows, throw blankets, bath towels'),
        (19, N'Tools & Hardware', N'Power tools, hand tools, toolboxes, extension cords, fasteners'),
        (20, N'Lawn & Garden Care', N'Lawn mowers, trimmers, garden hoses, planters, pots, yard tools'),
        (21, N'Sporting Goods & Fitness', N'Dumbbells, bicycles, golf clubs, tennis rackets, helmets, yoga mats'),
        (22, N'Camping & Outdoor Recreation', N'Tents, sleeping bags, coolers, lanterns, hiking gear'),
        (23, N'Crafts & Hobby Supplies', N'Sewing materials, yarn, paint sets, scrapbooking gear, fabric scraps'),
        (24, N'Holiday & Seasonal Decor', N'Christmas ornaments, Halloween props, holiday lights, wreaths'),
        (25, N'Pet Supplies', N'Dog crates, pet beds, leashes, carriers, unused pet toys/bowls')
    ) AS Source (Id, Name, Description)
    ON (Target.Id = Source.Id)
    
    WHEN MATCHED THEN
        UPDATE SET 
            Target.Name = Source.Name,
            Target.Description = Source.Description
            
    WHEN NOT MATCHED THEN
        INSERT (Id, Name, Description)
        VALUES (Source.Id, Source.Name, Source.Description);

    SET IDENTITY_INSERT [dbo].[ItemCategories] OFF;

    -------------------------------------------------------------------
    -- 2. SEED TABLE: GarageSaleTypes
    -------------------------------------------------------------------
    PRINT 'Seeding Garage Sale Types table...';
    
    SET IDENTITY_INSERT [dbo].[GarageSaleTypes] ON; 

    MERGE INTO [dbo].[GarageSaleTypes] WITH (HOLDLOCK) AS Target
    USING (VALUES 
        (1, N'Single-Family Sale', N'A standard garage or yard sale hosted by an individual household selling personal secondhand belongings.'),
        (2, N'Multi-Family Sale', N'A combined sale where two or more households pool items together at one location to offer greater variety.'),
        (3, N'Community / Neighborhood Sale', N'An organized event where multiple homes across a subdivision or neighborhood host sales on the same day.'),
        (4, N'Rummage Sale', N'A large-scale sale typically hosted by churches, schools, or non-profits selling donated items for fundraising.'),
        (5, N'Moving Sale', N'A high-volume sale aimed at liquidating belongings before relocating or transitioning to a new home.'),
        (6, N'Estate Sale', N'A comprehensive liquidation of an entire household''s contents, often managed professionally.'),
        (7, N'Bake Sale', N'An event selling homemade pastries, desserts, and snacks, usually benefiting a charity, school, or local organization.'),
        (8, N'Craft / Artisan Sale', N'A local marketplace featuring handmade crafts, artwork, custom goods, and DIY creations.'),
        (9, N'Charity / Benefit Sale', N'A dedicated fundraiser sale where all proceeds directly support a non-profit organization or local cause.'),
        (10, N'Storage Unit Sale', N'A liquidation sale offering items from self-storage lockers or unit buyouts.')
    ) AS Source (Id, Name, Description)
    ON (Target.Id = Source.Id)
    
    WHEN MATCHED THEN
        UPDATE SET 
            Target.Name = Source.Name,
            Target.Description = Source.Description
            
    WHEN NOT MATCHED THEN
        INSERT (Id, Name, Description)
        VALUES (Source.Id, Source.Name, Source.Description);

    SET IDENTITY_INSERT [dbo].[GarageSaleTypes] OFF;

    -------------------------------------------------------------------
    -- 3. SEED TABLE: Addresses
    -------------------------------------------------------------------
    PRINT 'Seeding Addresses table...';
    
    SET IDENTITY_INSERT [dbo].[Addresses] ON;

    MERGE INTO [dbo].[Addresses] WITH (HOLDLOCK) AS Target
    USING (VALUES 
        (1,  N'29 Buttermilk Dr',          N'Palm Coast', N'FL', N'32137', 29.5870, -81.2552),
        (2,  N'30 Fenwick Ln',             N'Palm Coast', N'FL', N'32137', 29.5907, -81.2257),
        (3,  N'12 Coolidge Ct',            N'Palm Coast', N'FL', N'32137', 29.5829, -81.2031),
        (4,  N'19 Botany Ln',              N'Palm Coast', N'FL', N'32137', 29.5675, -81.2303),
        (5,  N'29 Wellham Ln',             N'Palm Coast', N'FL', N'32164', 29.5500, -81.2548),
        (6,  N'85 Putter Dr',              N'Palm Coast', N'FL', N'32164', 29.5471, -81.2310),
        (7,  N'150 Laramie Dr',            N'Palm Coast', N'FL', N'32137', 29.6148, -81.2649),
        (8,  N'166 Bridgehaven Dr',        N'Palm Coast', N'FL', N'32137', 29.5641, -81.2450),
        (9,  N'3 Village Ln',              N'Palm Coast', N'FL', N'32164', 29.5415, -81.2415),
        (10, N'6 Wood Clift Ln',           N'Palm Coast', N'FL', N'32164', 29.5303, -81.2554),
        (11, N'43 Patric Dr',              N'Palm Coast', N'FL', N'32164', 29.5369, -81.2207),
        (12, N'3 Princess Kim Pl',         N'Palm Coast', N'FL', N'32164', 29.5198, -81.2233),
        (13, N'30 Russo Dr',               N'Palm Coast', N'FL', N'32164', 29.5155, -81.2466),
        (14, N'76 Port Royal Dr',          N'Palm Coast', N'FL', N'32164', 29.5071, -81.2022),
        (15, N'26 Rambling Ln',            N'Palm Coast', N'FL', N'32164', 29.5082, -81.2327),
        (16, N'43 Regency Dr',             N'Palm Coast', N'FL', N'32164', 29.5033, -81.2591),
        (17, N'73 Eric Dr',                N'Palm Coast', N'FL', N'32164', 29.4905, -81.2428),
        (18, N'9 Ellison Ln',              N'Palm Coast', N'FL', N'32164', 29.4890, -81.2274),
        (19, N'39 Primrose Ln',            N'Palm Coast', N'FL', N'32164', 29.5266, -81.2061),
        (20, N'12 Sea Board Ct',           N'Palm Coast', N'FL', N'32164', 29.4222, -81.1777),
        (21, N'10 Buttonwood Ln',          N'Palm Coast', N'FL', N'32137', 29.5918, -81.2542)
    ) AS Source (Id, Street, City, State, Zip, Lat, Lon)
    ON (Target.Id = Source.Id)

    WHEN MATCHED THEN
        UPDATE SET 
            Target.Street = Source.Street,
            Target.City = Source.City,
            Target.State = Source.State,
            Target.Zip = Source.Zip,
            Target.Lat = Source.Lat,
            Target.Lon = Source.Lon,
            Target.Location = geography::Point(Source.Lat, Source.Lon, 4326)

    WHEN NOT MATCHED THEN
        INSERT (Id, Street, City, State, Zip, Lat, Lon, Location)
        VALUES (
            Source.Id, 
            Source.Street, 
            Source.City, 
            Source.State, 
            Source.Zip, 
            Source.Lat, 
            Source.Lon, 
            geography::Point(Source.Lat, Source.Lon, 4326)
        );

    SET IDENTITY_INSERT [dbo].[Addresses] OFF;

    -------------------------------------------------------------------
    -- 4. SEED TABLE: Users
    -------------------------------------------------------------------
    PRINT 'Seeding Users table...';
    
    SET IDENTITY_INSERT [dbo].[Users] ON;

    MERGE INTO [dbo].[Users] WITH (HOLDLOCK) AS Target
    USING (VALUES 
        (1,  1,  N'jdoe',          N'Password381!', N'john.doe@example.com',        0),
        (2,  2,  N'jsmith',        N'Password742!', N'jane.smith@example.com',      0),
        (3,  3,  N'mallen',        N'Password159!', N'mark.allen@example.com',      0),
        (4,  4,  N'sbrown',        N'Password904!', N'sarah.brown@example.com',     0),
        (5,  5,  N'dwilson',       N'Password628!', N'david.wilson@example.com',    0),
        (6,  6,  N'emiller',       N'Password215!', N'emily.miller@example.com',    0),
        (7,  7,  N'cgarcia',       N'Password837!', N'carlos.garcia@example.com',   0),
        (8,  8,  N'lharris',       N'Password490!', N'lisa.harris@example.com',     0),
        (9,  9,  N'rjohnson',      N'Password613!', N'robert.johnson@example.com',  0),
        (10, 10, N'kclark',        N'Password375!', N'karen.clark@example.com',     0),
        (11, 11, N'tmartinez',     N'Password829!', N'tom.martinez@example.com',    0),
        (12, 12, N'arobinson',     N'Password504!', N'amy.robinson@example.com',    0),
        (13, 13, N'bwhite',        N'Password168!', N'brian.white@example.com',     0),
        (14, 14, N'hlee',          N'Password941!', N'heather.lee@example.com',     0),
        (15, 15, N'gwalker',       N'Password273!', N'greg.walker@example.com',     0),
        (16, 16, N'shall',         N'Password680!', N'stephanie.hall@example.com', 0),
        (17, 17, N'pyoung',        N'Password417!', N'peter.young@example.com',     0),
        (18, 18, N'aking',         N'Password852!', N'amanda.king@example.com',     0),
        (19, 19, N'jwright',       N'Password339!', N'jason.wright@example.com',    0),
        (20, 20, N'rscott',        N'Password716!', N'rachel.scott@example.com',    0),
        (21, 21, N'jchurch',       N'Password123!', N'jeffrey.church@example.com', 1)
    ) AS Source (Id, AddressId, UserName, Password, Email, IsAdmin)
    ON (Target.Id = Source.Id)

    WHEN MATCHED THEN
        UPDATE SET 
            Target.AddressId = Source.AddressId,
            Target.UserName = Source.UserName,
            Target.Password = Source.Password,
            Target.Email = Source.Email,
            Target.IsAdmin = Source.IsAdmin

    WHEN NOT MATCHED THEN
        INSERT (Id, AddressId, UserName, Password, Email, IsAdmin)
        VALUES (Source.Id, Source.AddressId, Source.UserName, Source.Password, Source.Email, Source.IsAdmin);

    SET IDENTITY_INSERT [dbo].[Users] OFF;

    -------------------------------------------------------------------
    -- 5. SEED TABLE: Notifications (10 Users with random MaxRadius)
    -------------------------------------------------------------------
    PRINT 'Seeding Notifications table...';
    
    SET IDENTITY_INSERT [dbo].[Notifications] ON;

    MERGE INTO [dbo].[Notifications] WITH (HOLDLOCK) AS Target
    USING (VALUES 
        (1,  1,  3),
        (2,  2,  5),
        (3,  3,  2),
        (4,  4,  4),
        (5,  5,  5),
        (6,  6,  1),
        (7,  7,  3),
        (8,  8,  5),
        (9,  9,  2),
        (10, 10, 4)
    ) AS Source (Id, UserId, MaxRadius)
    ON (Target.Id = Source.Id)

    WHEN MATCHED THEN
        UPDATE SET 
            Target.UserId = Source.UserId,
            Target.MaxRadius = Source.MaxRadius

    WHEN NOT MATCHED THEN
        INSERT (Id, UserId, MaxRadius)
        VALUES (Source.Id, Source.UserId, Source.MaxRadius);

    SET IDENTITY_INSERT [dbo].[Notifications] OFF;

    -------------------------------------------------------------------
    -- 6. SEED TABLE: GarageSales (10 Sales)
    -------------------------------------------------------------------
    PRINT 'Seeding GarageSales table...';
    
    SET IDENTITY_INSERT [dbo].[GarageSales] ON;

    MERGE INTO [dbo].[GarageSales] WITH (HOLDLOCK) AS Target
    USING (VALUES 
        (1,  1,  1,  1,  N'Huge multi-family downsizing sale! Vintage furniture, kitchenware, and tools.', 0),
        (2,  5,  2,  2,  N'Moving out of Palm Coast! Everything must go: bedroom sets, electronics, and books.', 0),
        (3,  3,  3,  3,  N'Grand Haven neighborhood community sale. Lots of kids clothes, toys, and baby gear.', 0),
        (4,  2,  5,  5,  N'Annual church rummage sale featuring crafts, home decor, and collectibles.', 0),
        (5,  6,  7,  7,  N'Estate sale offering antique china, framed artwork, and solid oak dining furniture.', 0),
        (6,  1,  13,  13,  N'Weekend garage sale with tons of power tools, garden equipment, and lawn mowers.', 0),
        (7,  8,  14,  14,  N'Artisan & craft sale featuring handmade jewelry, pottery, and holiday wreaths.', 0),
        (8,  9,  16,  16,  N'Sports enthusiast liquidation! Golf clubs, adult bicycles, dumbbells, and camping gear.', 0),
        (9,  4,  18,  18,  N'Back-to-school clothing drive and charity benefit sale. Great prices on kids items.', 0),
        (10, 10, 20, 20, N'Storage unit liquidation: vintage video games, vinyl records, and retro audio gear.', 0)
    ) AS Source (Id, SaleTypeId, OwnerId, AddressId, Description, Draft)
    ON (Target.Id = Source.Id)

    WHEN MATCHED THEN
        UPDATE SET 
            Target.SaleTypeId = Source.SaleTypeId,
            Target.OwnerId = Source.OwnerId,
            Target.AddressId = Source.AddressId,
            Target.Description = Source.Description,
            Target.Draft = Source.Draft

    WHEN NOT MATCHED THEN
        INSERT (Id, SaleTypeId, OwnerId, AddressId, Description, Draft)
        VALUES (Source.Id, Source.SaleTypeId, Source.OwnerId, Source.AddressId, Source.Description, Source.Draft);

    SET IDENTITY_INSERT [dbo].[GarageSales] OFF;

    -------------------------------------------------------------------
    -- 7. SEED TABLE: FeaturedItems (1-2 per Garage Sale)
    -------------------------------------------------------------------
    PRINT 'Seeding FeaturedItems table...';
    
    SET IDENTITY_INSERT [dbo].[FeaturedItems] ON;

    MERGE INTO [dbo].[FeaturedItems] WITH (HOLDLOCK) AS Target
    USING (VALUES 
        (1,  1,  16, N'Solid Oak 6-Drawer Dresser in excellent condition'),
        (2,  1,  13, N'Ninja Air Fryer Max XL - barely used'),
        (3,  2,  10, N'55-inch LG 4K Smart TV with wall mount'),
        (4,  2,  16, N'Queen size mattress frame and headboard'),
        (5,  3,  7,  N'LEGO Star Wars Millennium Falcon set (100% complete)'),
        (6,  3,  3,  N'Gently used toddler outerwear and shoes (sizes 2T-4T)'),
        (7,  4,  15, N'Handcrafted wooden wall clocks and framed canvas art'),
        (8,  5,  14, N'Vintage 12-piece porcelain tea set'),
        (9,  5,  15, N'19th Century antique carved wooden mirror'),
        (10, 6,  19, N'DeWalt 20V Max Cordless Drill & Impact Driver Combo Set'),
        (11, 6,  20, N'Honda 21-inch Self-Propelled Gas Lawn Mower'),
        (12, 7,  5,  N'Handmade beaded necklaces and silver costume jewelry'),
        (13, 8,  21, N'Callaway Golf Club Set with Stand Bag'),
        (14, 8,  21, N'Trek FX 2 Hybrid Road Bike - Medium Frame'),
        (15, 9,  12, N'Full collection of Harry Potter hardcover books'),
        (16, 10, 9,  N'Nintendo 64 Console with 2 controllers and Super Mario 64'),
        (17, 10, 11, N'Collection of 1970s Classic Rock vinyl records (30+ LPs)')
    ) AS Source (Id, GarageSaleId, CategoryId, Description)
    ON (Target.Id = Source.Id)

    WHEN MATCHED THEN
        UPDATE SET 
            Target.GarageSaleId = Source.GarageSaleId,
            Target.CategoryId = Source.CategoryId,
            Target.Description = Source.Description

    WHEN NOT MATCHED THEN
        INSERT (Id, GarageSaleId, CategoryId, Description)
        VALUES (Source.Id, Source.GarageSaleId, Source.CategoryId, Source.Description);

    SET IDENTITY_INSERT [dbo].[FeaturedItems] OFF;

    -------------------------------------------------------------------
    -- 8. SEED TABLE: GarageSaleSchedules 
    -- (Strict Rule: All schedules for a sale fall within 2 consecutive days)
    -------------------------------------------------------------------
    PRINT 'Seeding GarageSaleSchedules table...';
    
    SET IDENTITY_INSERT [dbo].[GarageSaleSchedules] ON;

    MERGE INTO [dbo].[GarageSaleSchedules] WITH (HOLDLOCK) AS Target
    USING (VALUES 
        -- Sale 1: Friday Aug 14 & Saturday Aug 15, 2026
        (1,  1,  '2026-08-14 08:00:00', '2026-08-14 14:00:00'),
        (2,  1,  '2026-08-15 08:00:00', '2026-08-15 12:00:00'),

        -- Sale 2: Saturday Aug 15 & Sunday Aug 16, 2026
        (3,  2,  '2026-08-15 07:00:00', '2026-08-15 15:00:00'),
        (4,  2,  '2026-08-16 08:00:00', '2026-08-16 13:00:00'),

        -- Sale 3: Saturday Aug 15, 2026 (Single day)
        (5,  3,  '2026-08-15 08:30:00', '2026-08-15 16:00:00'),

        -- Sale 4: Friday Aug 21 & Saturday Aug 22, 2026
        (6,  4,  '2026-08-21 09:00:00', '2026-08-21 17:00:00'),
        (7,  4,  '2026-08-22 09:00:00', '2026-08-22 14:00:00'),

        -- Sale 5: Saturday Aug 22 & Sunday Aug 23, 2026
        (8,  5,  '2026-08-22 08:00:00', '2026-08-22 16:00:00'),
        (9,  5,  '2026-08-23 09:00:00', '2026-08-23 14:00:00'),

        -- Sale 6: Saturday Aug 22, 2026 (Single day)
        (10, 6,  '2026-08-22 07:00:00', '2026-08-22 13:00:00'),

        -- Sale 7: Friday Aug 28 & Saturday Aug 29, 2026
        (11, 7,  '2026-08-28 10:00:00', '2026-08-28 18:00:00'),
        (12, 7,  '2026-08-29 09:00:00', '2026-08-29 15:00:00'),

        -- Sale 8: Saturday Aug 29 & Sunday Aug 30, 2026
        (13, 8,  '2026-08-29 07:30:00', '2026-08-29 14:00:00'),
        (14, 8,  '2026-08-30 08:00:00', '2026-08-30 12:00:00'),

        -- Sale 9: Saturday Aug 29, 2026 (Single day)
        (15, 9,  '2026-08-29 08:00:00', '2026-08-29 15:00:00'),

        -- Sale 10: Friday Sep 4 & Saturday Sep 5, 2026
        (16, 10, '2026-09-04 08:00:00', '2026-09-04 14:00:00'),
        (17, 10, '2026-09-05 08:00:00', '2026-09-05 12:00:00')
    ) AS Source (Id, GarageSaleId, [From], [To])
    ON (Target.Id = Source.Id)

    WHEN MATCHED THEN
        UPDATE SET 
            Target.GarageSaleId = Source.GarageSaleId,
            Target.[From] = Source.[From],
            Target.[To] = Source.[To]

    WHEN NOT MATCHED THEN
        INSERT (Id, GarageSaleId, [From], [To])
        VALUES (Source.Id, Source.GarageSaleId, Source.[From], Source.[To]);

    SET IDENTITY_INSERT [dbo].[GarageSaleSchedules] OFF;

    -- Commit transaction if all table seeds complete successfully
    COMMIT TRANSACTION;
    PRINT 'Master database seeding completed successfully.';

END TRY
BEGIN CATCH
    -- Ensure IDENTITY_INSERT is disabled on all target tables upon failure
    SET IDENTITY_INSERT [dbo].[ItemCategories] OFF;
    SET IDENTITY_INSERT [dbo].[GarageSaleTypes] OFF;
    SET IDENTITY_INSERT [dbo].[Addresses] OFF;
    SET IDENTITY_INSERT [dbo].[Users] OFF;

    -- Roll back all operations if any statement fails
    IF @@TRANCOUNT > 0 ROLLBACK TRANSACTION;
    
    PRINT 'Master seeding failed. Error details:';
    SELECT 
        ERROR_NUMBER() AS ErrorNumber,
        ERROR_MESSAGE() AS ErrorMessage;
END CATCH;