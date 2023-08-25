# App fő funkciókörök

- Refresh datasources: letöltések, parsolások. Az eredmény JSON-ba szerializálva, lokálisan tárolva.
  Ezeket az adatforrásokat egyesével is lehet frissíteni? A minden téma letöltését lehessen külön futtatni...
- Mi van akkor, amikor bizonyos adatforrások még nem állnak rendelkezésre? (Pl. félév közben a jegyek...)
- Figyelő üzemmód minden időszakra: csak azt frissíti, ami olyankor tipikusan változik és mutatja a lényeget.
- Átfogó report generálás publikálható formában
- Hibakeresés: hibás jelentkezések, elveszett hallgatók stb.
- Osztályzatok bevitele a Neptunba és közben anomáliák kezelése

Ezek a kimutatások menjenek kiadható Excel táblákba?!

Hibajelzések Excel körlevél funkcióval??

# Félév indítás előkészítése

## Ki van már írva elegendő téma? Kit kell még pingelni?

TopicCountPerStudentGroup: hallgatói csoportonként a kiírt témák száma (létszámtól függetlenül)
SeatCountPerStudentGroup: hallgatói csoportonként a kiírt helyek száma
SeatCountPerAdvisor: Konzulensenként a kiírt helyek száma (külön magyar és angol és összesen)

??? Több konzulenses témáknál kinél hány szabad helyet számolunk? Mindenkinél szabad annyi hely???

# Félév elején, első 3 hétben

## A hallgatók mekkora része jelentkezett már témára? Mennyi szabad hely van még az egyes hallgatói kategóriák számára?

SeatOccupancy: Minden Neptunban szereplő hallgatóból hányan jelentkeztek már témára (külön magyar és angol)
SeatOccupancyPerStudentGroup: hallgatói csoportonként a foglalt helyek száma
SeatOccupancyPerAdvisor: Konzulensenként a foglalt helyek aránya (külön magyar és angol)

## Ha valaki keres konzulenst, kit javasolhatok?

RatioOfTopicsWithFreeSeats: a témák mekkora részében van még szabad hely?
WhoHasFreeSeats: kinél hány szabad hely van még?

# Beszámoló szervezéshez közeledve

WhoDoesNotHaveTopic: melyik hallgatóknak nincs még témája?
WhoIsMissingInNeptun: melyik témán lévő hallgatók nem szerepelnek a Neptunban?

TopicsWithUndefinedSessionType: melyik témának nincs még szekció kategóriája? (SW, HW, ...)

# Beszámoló szekciók létrehozása

## Milyen szekcióból hány darab legyen?

NeededSectionCountByCategory: melyik szekció kategóriában hány szekció kellene?

# Beszámoló szervezés


## Moodle peer review előkészítés

MoodleWorkshopAdvisorGroupList: konzulens csoportok létrehozása

MoodleWorkshopSessionGroupList: szekció csoportok létrehozása

## Beszámolók előtt

WhoIsNotInASessionYet: kik nem kerültek még be egy szekcióba sem?



# Beszámolók után eredmények beírása

Ki nem volt beszámolón?
Ki értékelt szövegesen is a Moodle peer review-ban?

## Pingelés, hogy ki nem írta még be a jegyeket

WhoDidNotAssignGradesYet: kik nem írták még be a jegyeket?

## Jegyek beírása a Neptunba

NeptunGradeImport: jegyek a Neptunba importáláshoz

# Egyebek

Külső témán lévő hallgatók aránya?

---------------------
            Console.WriteLine("========== Statistics ==========");
            Console.WriteLine($"Topic count: {Context.Topics.Count()}");
            Console.WriteLine($"External topic count: {Context.Topics.Count(t=>t.IsExternal)}");

            Console.WriteLine("====== Course based statistics: (total, occupied, and available seat counts for hungarian and english students)");
            foreach(var cc in Context.CourseCategories)
            {
                Console.WriteLine($"--- Course Category: {cc.Title}");
                Console.WriteLine($"Number of enrolled students: hungarian {cc.enrolledHungarianStudentCount}, english {cc.enrolledEnglishStudentCount}");
                var hunTopics = Context.Topics.Where(t => t.CourseCategories.Contains(cc.Title)).Where(t => !t.Title.StartsWith("Z-Eng")).ToArray();
                var engTopics = Context.Topics.Where(t => t.CourseCategories.Contains(cc.Title)).Where(t => t.Title.StartsWith("Z-Eng")).ToArray();
                var hunSeatCount = hunTopics.Sum(t => t.MaxStudentCount);
                var engSeatCount = engTopics.Sum(t => t.MaxStudentCount);
                //Console.WriteLine($"Total seat count HUN {hunSeatCount}, ENG {engSeatCount}");
                var hunOccupiedSeatCount = hunTopics.Sum(t => t.StudentNKods.Count);
                var engOccupiedSeatCount = engTopics.Sum(t => t.StudentNKods.Count);
                //Console.WriteLine($"Occupied seat count HUN {hunOccupiedSeatCount}, ENG {engOccupiedSeatCount}");
                Console.WriteLine($"Used seat ratios HUN {100 * hunOccupiedSeatCount / hunSeatCount}% ENG { ((engSeatCount>0) ? (100*engOccupiedSeatCount/engSeatCount):(0)) }% ");
            }

            // Melyik konzulensnél van még szabad hely (magyar-angol helyeket nem megkülönböztetve)
            foreach(var advisor in availableSeats.Keys)
            {
                Console.WriteLine($"Advisor {advisor} free seats: {availableSeats[advisor]}");
            }
            


            // Konzulensenként: mennyi szabad hely van és ez melyik kurzus kategóriákra vonatkozik (és angol vagy magyar)?
            //  Közös témán lévő szabad hely minden konzulenshez számítson ebben az esetben, de egyébként a szabad helyek számába csak egyszer!

            // Angol hallgatók (Neptun kurzus szerint) magyar kiírású témán? (Kell a Neptun kurzusok exportja egyesével)

            // Félév elejére:
            // Hány hallgatónak nincsen még témája? Ezek mely kurzusokon vannak?
            //  Ehhez kell a Neptun névsor minden tantárgyhoz

            // Csak érdekességként:
            // Külső témákon lévők aránya kurzus kategóriánként
            // Kiírt helyek száma belső és külső témákon?

            // Beszámolókon ki nem jelentkezett még felügyelőnek 2 (3) helyre?

            // Félév végére:
            // Utána igazából már a jegyeket is összeszedheti ez a rendszer, többszörös jelentkezésekre jobban felkészülve, mint a másik.

            Console.WriteLine("====== Further stats");
            Console.WriteLine($"Number or enrolled students: {Context.Topics.Count(t => t.IsExternal)}");
            Console.WriteLine($"Topics with multiple advisors: {Context.Topics.Count(t => t.Advisors.Count > 1)}");
            Console.WriteLine($"Topics with multiple courses: {Context.Topics.Count(t => t.CourseCategories.Count > 1)}");

            Console.WriteLine($"Total capacity for students: {Context.Topics.Sum(t => t.MaxStudentCount)}");
            Console.WriteLine($"Total free capacity for students: {Context.Topics.Sum(t => t.MaxStudentCount - t.StudentNKods.Count)}");
            Console.WriteLine($"Number of topics with free seats: {Context.Topics.Count(t => t.MaxStudentCount > t.StudentNKods.Count)}");

