namespace AmThucQuan4.Native.Services;

/// <summary>
/// Quản lý ngôn ngữ toàn app.
/// Nội dung đa ngôn ngữ cho 5 POIs Đoàn Văn Bơ.
/// </summary>
public class LanguageService : ILanguageService
{
    private string _currentCode = "vi";
    public event Action? LanguageChanged;

    public string CurrentCode => _currentCode;
    public string CurrentName => Available.First(l => l.Code == _currentCode).Name;
    public string CurrentFlag => Available.First(l => l.Code == _currentCode).Flag;
    public string? TtsLocale  => Available.First(l => l.Code == _currentCode).TtsLocale;

    public List<LanguageOption> Available { get; } =
    [
        new("vi", "Tiếng Việt", "🇻🇳", "vi-VN"),
        new("en", "English",    "🇬🇧", "en-US"),
        new("fr", "Français",   "🇫🇷", "fr-FR"),
        new("zh", "中文",        "🇨🇳", "zh-CN"),
        new("ja", "日本語",      "🇯🇵", "ja-JP"),
    ];

    public void SetLanguage(string code)
    {
        if (_currentCode == code) return;
        _currentCode = code;
        LanguageChanged?.Invoke();
    }

    // ─── Nội dung đa ngôn ngữ cho từng POI ──────────────────────
    private static readonly Dictionary<string, Dictionary<string, (string Name, string Desc, string Script, string Address)>>
        _i18n = new()
    {
        ["poi-1"] = new()
        {
            ["vi"] = ("Cơm Tấm Bà Út",
                "Cơm tấm sườn bì chả chuẩn vị Sài Gòn, nước mắm pha gia truyền.",
                "Chào mừng đến Cơm Tấm Bà Út. Sườn nướng than hoa thơm lừng, bì trộn sợi mỏng. Giá từ 35 đến 65 nghìn đồng.",
                "32 Hoàng Diệu, Phường 10, Quận 4, TP.HCM"),

            ["en"] = ("Mrs. Ut's Broken Rice",
                "Classic Saigon-style broken rice with grilled pork, pork skin and steamed egg cake.",
                "Welcome to Mrs. Ut's Broken Rice. Charcoal-grilled pork chop with 30-year family fish sauce recipe. Prices from 35,000 to 65,000 VND.",
                "32 Hoang Dieu Street, Ward 10, District 4, Ho Chi Minh City"),

            ["fr"] = ("Riz Brisé de Madame Út",
                "Riz brisé à la mode de Saïgon avec côtelette grillée et gâteau d'œuf.",
                "Bienvenue chez Madame Út. Riz brisé traditionnel de Saïgon. Prix entre 35 000 et 45 000 VND.",
                "32 Rue Hoang Dieu, Quartier 10, Arrondissement 4, Hô Chi Minh-Ville"),

            ["zh"] = ("乌大妈碎米饭",
                "正宗西贡风味碎米饭，配烤猪排、猪皮丝和蒸蛋糕。",
                "欢迎来到乌大妈碎米饭。炭火烤猪排配传承三十年的家传鱼露。价格35000到65000越南盾。",
                "第4郡第10坊黄耀街32号，胡志明市"),

            ["ja"] = ("ウーおばさんのコムタム",
                "サイゴン伝統のコムタム。炭火焼きポークチョップ、豚皮の細切り、蒸し卵ケーキ。",
                "ウーおばさんのコムタムへようこそ。30年受け継がれた秘伝ヌクマムソース。価格は35,000〜65,000ドン。",
                "ホーチミン市第4区第10坊ホアンジエウ通り32番地"),
        },

        ["poi-2"] = new()
        {
            ["vi"] = ("Bánh Mì Huỳnh Hoa",
                "Bánh mì giòn rụm nhân đầy chả lụa, thịt nguội, pa-tê nổi tiếng Sài Gòn.",
                "Chào mừng đến Bánh Mì Huỳnh Hoa. Ổ bánh giòn, nhân đầy pa-tê béo ngậy. Giá từ 30 đến 45 nghìn đồng.",
                "26 Đoàn Văn Bơ, Phường 13, Quận 4, TP.HCM"),

            ["en"] = ("Huynh Hoa Baguette",
                "Famous Saigon-style baguette packed with Vietnamese sausage, cold cuts and pâté.",
                "Welcome to Huynh Hoa Baguette. Crispy baguette loaded with sausage and creamy pâté. Prices from 30,000 to 45,000 VND.",
                "26 Doan Van Bo Street, Ward 13, District 4, Ho Chi Minh City"),

            ["fr"] = ("Bánh Mì Huỳnh Hoa",
                "Baguette vietnamienne croustillante garnie de saucisse, charcuterie et pâté.",
                "Bienvenue chez Huỳnh Hoa. Baguette croustillante garnie de pâté onctueux. Prix entre 30 000 et 45 000 VND.",
                "26 Rue Doan Van Bo, Quartier 13, Arrondissement 4, Hô Chi Minh-Ville"),

            ["zh"] = ("黄花面包",
                "著名的西贡法棍，内馅丰盛，有越式香肠、冷肉和猪肝酱。",
                "欢迎来到黄花面包。外皮酥脆法棍塞满猪肝酱。价格30000到45000越南盾。",
                "胡志明市第4郡第13坊段文波街26号"),

            ["ja"] = ("フィン・ホア・バインミー",
                "サイゴン名物のバゲット。ベトナムソーセージ、コールドカット、パテが詰め込まれた人気店。",
                "フィン・ホアへようこそ。カリカリバゲットにパテたっぷり。価格は30,000〜45,000ドン。",
                "ホーチミン市第4区第13坊ドアンバンボ通り26番地"),
        },

        ["poi-3"] = new()
        {
            ["vi"] = ("Ốc Đào",
                "Quán ốc vỉa hè nổi tiếng với ốc len xào dừa và nghêu hấp sả.",
                "Chào mừng đến Ốc Đào. Ốc len xào dừa béo ngậy, nghêu hấp sả thơm lừng. Giá từ 50 đến 150 nghìn đồng.",
                "5 Đoàn Văn Bơ, Phường 12, Quận 4, TP.HCM"),

            ["en"] = ("Dao Seafood",
                "Famous street-side seafood spot known for coconut stir-fried snails and lemongrass clams.",
                "Welcome to Dao Seafood. Coconut stir-fried snails and lemongrass steamed clams. Prices from 50,000 to 150,000 VND.",
                "5 Doan Van Bo Street, Ward 12, District 4, Ho Chi Minh City"),

            ["fr"] = ("Ốc Đào — Fruits de mer",
                "Célèbre restaurant de rue spécialisé dans les escargots sautés à la noix de coco.",
                "Bienvenue chez Ốc Đào. Escargots sautés à la noix de coco et palourdes à la citronnelle. Prix entre 50 000 et 150 000 VND.",
                "5 Rue Doan Van Bo, Quartier 12, Arrondissement 4, Hô Chi Minh-Ville"),

            ["zh"] = ("桃螺蛳店",
                "著名的街边海鲜摊，以椰汁炒螺蛳和香茅蛤蜊闻名。",
                "欢迎来到桃螺蛳店。招牌椰汁炒螺蛳和香茅蒸蛤蜊。价格50000到150000越南盾。",
                "胡志明市第4郡第12坊段文波街5号"),

            ["ja"] = ("ダオ貝料理店",
                "ディストリクト4の有名な路上海鮮店。ココナッツ炒めカタツムリが名物。",
                "ダオ貝料理店へようこそ。ココナッツソース炒めカタツムリとレモングラス蒸しアサリ。価格は50,000〜150,000ドン。",
                "ホーチミン市第4区第12坊ドアンバンボ通り5番地"),
        },

        ["poi-4"] = new()
        {
            ["vi"] = ("Trà Sữa Phúc Long",
                "Thương hiệu trà sữa Việt nổi tiếng với trà ô long và cà phê đặc trưng.",
                "Chào mừng đến Phúc Long. Trà ô long sữa đậm thơm, cà phê phin truyền thống. Giá từ 29 đến 65 nghìn đồng.",
                "10 Đoàn Văn Bơ, Phường 12, Quận 4, TP.HCM"),

            ["en"] = ("Phuc Long Tea & Coffee",
                "Iconic Vietnamese tea and coffee brand with over 60 years of history.",
                "Welcome to Phuc Long. Signature milk oolong tea and traditional drip coffee. Prices from 29,000 to 65,000 VND.",
                "10 Doan Van Bo Street, Ward 12, District 4, Ho Chi Minh City"),

            ["fr"] = ("Phúc Long Thé et Café",
                "Marque vietnamienne iconique de thé et café avec plus de 60 ans d'histoire.",
                "Bienvenue chez Phúc Long. Thé au lait oolong et café filtré traditionnel. Prix entre 29 000 et 65 000 VND.",
                "10 Rue Doan Van Bo, Quartier 12, Arrondissement 4, Hô Chi Minh-Ville"),

            ["zh"] = ("福隆茶饮",
                "拥有超过60年历史的越南著名茶饮品牌。",
                "欢迎来到福隆。招牌乌龙奶茶浓郁芬芳，传统滴漏咖啡醇厚。价格29000到65000越南盾。",
                "胡志明市第4郡第12坊段文波街10号"),

            ["ja"] = ("フック・ロン ティー＆コーヒー",
                "創業60年以上のベトナムを代表するお茶とコーヒーのブランド。",
                "フック・ロンへようこそ。ミルクウーロンティーとトラディショナルドリップコーヒー。価格は29,000〜65,000ドン。",
                "ホーチミン市第4区第12坊ドアンバンボ通り10番地"),
        },

        ["poi-5"] = new()
        {
            ["vi"] = ("Phở 24",
                "Phở bò nước dùng trong vắt ninh từ xương bò tươi 8 tiếng.",
                "Chào mừng đến Phở 24. Nước dùng ninh 8 tiếng từ xương ống bò tươi. Mở đến 10 giờ rưỡi sáng. Giá từ 45 đến 75 nghìn đồng.",
                "8 Nguyễn Tất Thành, Phường 13, Quận 4, TP.HCM"),

            ["en"] = ("Pho 24",
                "Vietnamese beef noodle soup with clear broth simmered from fresh beef bones for 8 hours.",
                "Welcome to Pho 24. Crystal-clear broth simmered for 8 hours. Open until 10:30 AM. Prices from 45,000 to 75,000 VND.",
                "8 Nguyen Tat Thanh Street, Ward 13, District 4, Ho Chi Minh City"),

            ["fr"] = ("Pho 24",
                "Soupe de nouilles au bœuf vietnamienne avec un bouillon mijoté pendant 8 heures.",
                "Bienvenue chez Pho 24. Bouillon mijoté 8 heures. Ouvert jusqu'à 10h30. Prix entre 45 000 et 75 000 VND.",
                "8 Rue Nguyen Tat Thanh, Quartier 13, Arrondissement 4, Hô Chi Minh-Ville"),

            ["zh"] = ("24号河粉",
                "越南牛肉河粉，牛骨高汤熬制8小时，汤清味醇。",
                "欢迎来到24号河粉。新鲜牛骨熬制8小时高汤。营业至上午10点半。价格45000到75000越南盾。",
                "胡志明市第4郡第13坊阮必成街8号"),

            ["ja"] = ("フォー24",
                "新鮮な牛骨を8時間煮込んだ澄んだスープのベトナム牛肉麺。",
                "フォー24へようこそ。新鮮牛骨を8時間煮込んだスープ。午前10時半まで営業。価格は45,000〜75,000ドン。",
                "ホーチミン市第4区第13坊グエンタットタイン通り8番地"),
        },
    };

    public string GetName(string poiId, string fallback)
        => GetField(poiId, _currentCode)?.Name ?? fallback;

    public string GetDescription(string poiId, string fallback)
        => GetField(poiId, _currentCode)?.Desc ?? fallback;

    public string GetAudioScript(string poiId, string fallback)
        => GetField(poiId, _currentCode)?.Script ?? fallback;

    public string GetAddress(string poiId, string fallback)
        => GetField(poiId, _currentCode)?.Address ?? fallback;

    private (string Name, string Desc, string Script, string Address)? GetField(
        string poiId, string code)
    {
        if (!_i18n.TryGetValue(poiId, out var langs)) return null;
        if (!langs.TryGetValue(code, out var content)) return null;
        return content;
    }

    // ─── UI Strings i18n ────────────────────────────────────────
    private static readonly Dictionary<string, Dictionary<string, string>> _ui = new()
    {
        ["nearby_places"] = new()
        {
            ["vi"] = "Địa điểm gần bạn",
            ["en"] = "Nearby Places",
            ["fr"] = "Lieux à proximité",
            ["zh"] = "附近地点",
            ["ja"] = "近くのスポット",
        },
        ["restaurants_count"] = new()
        {
            ["vi"] = "quán",
            ["en"] = "places",
            ["fr"] = "lieux",
            ["zh"] = "家",
            ["ja"] = "件",
        },
        ["search_placeholder"] = new()
        {
            ["vi"] = "Tìm địa chỉ hoặc quán ăn...",
            ["en"] = "Search address or restaurant...",
            ["fr"] = "Rechercher une adresse ou un restaurant...",
            ["zh"] = "搜索地址或餐厅...",
            ["ja"] = "住所またはレストランを検索...",
        },
        ["navigate"] = new()
        {
            ["vi"] = "🗺️ Dẫn đường",
            ["en"] = "🗺️ Navigate",
            ["fr"] = "🗺️ Itinéraire",
            ["zh"] = "🗺️ 导航",
            ["ja"] = "🗺️ ナビ",
        },
        ["play_audio"] = new()
        {
            ["vi"] = "▶ Phát Audio",
            ["en"] = "▶ Play Audio",
            ["fr"] = "▶ Écouter",
            ["zh"] = "▶ 播放",
            ["ja"] = "▶ 再生",
        },
        ["stop_audio"] = new()
        {
            ["vi"] = "⏹ Dừng",
            ["en"] = "⏹ Stop",
            ["fr"] = "⏹ Arrêter",
            ["zh"] = "⏹ 停止",
            ["ja"] = "⏹ 停止",
        },
        ["hours_label"] = new()
        {
            ["vi"] = "⏰",
            ["en"] = "⏰",
            ["fr"] = "⏰",
            ["zh"] = "⏰",
            ["ja"] = "⏰",
        },
        ["price_label"] = new()
        {
            ["vi"] = "💰",
            ["en"] = "💰",
            ["fr"] = "💰",
            ["zh"] = "💰",
            ["ja"] = "💰",
        },
        ["address_label"] = new()
        {
            ["vi"] = "📌",
            ["en"] = "📍",
            ["fr"] = "📍",
            ["zh"] = "📍",
            ["ja"] = "📍",
        },
        ["gps_button"] = new()
        {
            ["vi"] = "Đang lấy vị trí...",
            ["en"] = "Getting location...",
            ["fr"] = "Localisation...",
            ["zh"] = "定位中...",
            ["ja"] = "位置取得中...",
        },
        ["loading"] = new()
        {
            ["vi"] = "🍜 Đang tải Ẩm thực Quận 4...",
            ["en"] = "🍜 Loading District 4 Food Tour...",
            ["fr"] = "🍜 Chargement du guide gastronomique...",
            ["zh"] = "🍜 加载第4区美食指南...",
            ["ja"] = "🍜 4区グルメガイドを読み込み中...",
        },
        ["choose_language"] = new()
        {
            ["vi"] = "🌐 Chọn ngôn ngữ thuyết minh",
            ["en"] = "🌐 Select commentary language",
            ["fr"] = "🌐 Choisir la langue du commentaire",
            ["zh"] = "🌐 选择解说语言",
            ["ja"] = "🌐 ガイド言語を選択",
        },
    };

    public string Ui(string key)
    {
        if (_ui.TryGetValue(key, out var langs))
            if (langs.TryGetValue(_currentCode, out var val))
                return val;
        return key; // fallback = key name
    }
}
