using System;
using System.Collections;
using BepInEx;
using BepInEx.Configuration;
using HarmonyLib;
using RogueLibsCore;

namespace CatNoirRing
{
    [BepInPlugin(PluginGuid, PluginName, PluginVersion)]
    [BepInDependency(RogueLibs.GUID)]
    public class CatNoirPlugin : BaseUnityPlugin
    {
        public const string PluginGuid = "catnoir.ring";
        public const string PluginName = "Cat Noir Ring";
        public const string PluginVersion = "1.7.40";

        public static CatNoirPlugin? Live { get; private set; }

        public static ConfigEntry<int> FormDuration = null!;
        public static ConfigEntry<int> FormCooldown = null!;
        public static ConfigEntry<int> StaffDamage = null!;
        public static ConfigEntry<int> CataclysmDamage = null!;
        public static ConfigEntry<int> CloutDamage = null!;
        public static ConfigEntry<int> YoYoDamage = null!;
        public static ConfigEntry<int> MultiplyDuration = null!;
        public static ConfigEntry<int> ParalysisDuration = null!;
        public static ConfigEntry<int> SecondChanceDuration = null!;
        public static ConfigEntry<int> SublimationDuration = null!;
        public static ConfigEntry<int> ConfusionDuration = null!;
        public static ConfigEntry<int> DragonDuration = null!;
        public static ConfigEntry<int> PeacockDuration = null!;
        public static ConfigEntry<int> ButterflyDuration = null!;
        public static ConfigEntry<int> TimeStopDuration = null!;

        public static void AfterFrame(Action work)
        {
            if (Live == null || work == null)
                return;

            Live.StartCoroutine(Live.Again(work));
        }

        IEnumerator Again(Action work)
        {
            yield return new UnityEngine.WaitForEndOfFrame();
            work();
            yield return new UnityEngine.WaitForEndOfFrame();
            work();
        }

        void Awake()
        {
            Live = this;
            FormDuration = Config.Bind("General", "FormDuration", 15, "Сколько секунд длится форма суперкота.");
            FormCooldown = Config.Bind("General", "FormCooldown", 20, "Сколько секунд кольцо молчит после окончания формы.");
            StaffDamage = Config.Bind("General", "StaffDamage", 8, "Урон удара шеста, как у полицейской дубинки.");
            CataclysmDamage = Config.Bind("General", "CataclysmDamage", 30, "Урон катаклизма по существу.");
            CloutDamage = Config.Bind("General", "CloutDamage", 30, "Урон удара тигрицы по существу.");
            YoYoDamage = Config.Bind("General", "YoYoDamage", 10, "Базовый урон йо-йо и тромпо, как у сюрикена.");
            MultiplyDuration = Config.Bind("General", "MultiplyDuration", 60, "Сколько секунд длится умножение мыши.");
            ParalysisDuration = Config.Bind("General", "ParalysisDuration", 15, "Сколько секунд Веном держит цель в параличе.");
            SecondChanceDuration = Config.Bind("General", "SecondChanceDuration", 30, "Сколько секунд даёт Второй шанс змеи, прежде чем костюм спадёт.");
            SublimationDuration = Config.Bind("General", "SublimationDuration", 60, "Сколько секунд костюм петуха держится после выбора черты.");
            ConfusionDuration = Config.Bind("General", "ConfusionDuration", 15, "Сколько секунд смятение обезьяны переворачивает шаг и мешает идти.");
            DragonDuration = Config.Bind("General", "DragonDuration", 60, "Сколько секунд костюм дракона держится после первого оружия.");
            PeacockDuration = Config.Bind("General", "PeacockDuration", 60, "Сколько секунд костюм павлина держится после призыва союзника.");
            ButterflyDuration = Config.Bind("General", "ButterflyDuration", 60, "Сколько секунд костюм бабочки держится после превращения.");
            TimeStopDuration = Config.Bind("General", "TimeStopDuration", 10, "Сколько секунд кролик держит время остановленным.");

            RogueLibs.CreateCustomItem<CatNoirRingItem>()
                .WithName(new CustomNameInfo("Cat Noir Ring")
                {
                    Russian = "Кольцо Cat Noir",
                })
                .WithDescription(new CustomNameInfo("Claws out. Use again to change back. After Cataclysm, the suit lasts 15 seconds.")
                {
                    Russian = "Выпусти когти. Ещё раз используй кольцо, чтобы снять костюм. После катаклизма форма держится 15 секунд.",
                })
                .WithUnlock()
                .WithSprite(CatSprites.Ring, 32f);

            RogueLibs.CreateCustomItem<LadybugEarringsItem>()
                .WithName(new CustomNameInfo("Ladybug Earrings")
                {
                    Russian = "Серьги Ледибаг",
                })
                .WithDescription(new CustomNameInfo("Spots on. Use again to change back. After Lucky Charm, the suit lasts 15 seconds.")
                {
                    Russian = "Надень пятна. Ещё раз используй серьги, чтобы снять костюм. После Лаки Чарма форма держится 15 секунд.",
                })
                .WithUnlock()
                .WithSprite(CatSprites.Earrings, 32f);

            RogueLibs.CreateCustomItem<MouseTalismanItem>()
                .WithName(new CustomNameInfo("Mouse Talisman")
                {
                    Russian = "Талисман мыши",
                })
                .WithDescription(new CustomNameInfo("Become the mouse. Multiplication makes you tiny and calls three tiny allies for one minute.")
                {
                    Russian = "Стань мышью. Умножение уменьшает тебя и зовёт трёх мелких союзников. Через минуту костюм спадает.",
                })
                .WithUnlock()
                .WithSprite(CatSprites.MouseTalisman, 32f);

            RogueLibs.CreateCustomItem<BeeTalismanItem>()
                .WithName(new CustomNameInfo("Bee Talisman")
                {
                    Russian = "Талисман пчелы",
                })
                .WithDescription(new CustomNameInfo("Become the bee. Venom paralyzes one creature. The suit then lasts 15 seconds.")
                {
                    Russian = "Стань пчелой. Веном парализует одно существо. После этого костюм держится 15 секунд.",
                })
                .WithUnlock()
                .WithSprite(CatSprites.BeeTalisman, 32f);

            RogueLibs.CreateCustomItem<SnakeTalismanItem>()
                .WithName(new CustomNameInfo("Snake Talisman")
                {
                    Russian = "Талисман змеи",
                })
                .WithDescription(new CustomNameInfo("Become the snake. Second Chance marks the world, then you have 30 seconds to rewind back to it.")
                {
                    Russian = "Стань змеёй. Второй шанс запоминает мир. Пока идут 30 секунд, ещё одно нажатие возвращает всё к этому моменту и снова ставит 30 секунд.",
                })
                .WithUnlock()
                .WithSprite(CatSprites.SnakeTalisman, 32f);

            RogueLibs.CreateCustomItem<HorseTalismanItem>()
                .WithName(new CustomNameInfo("Horse Talisman")
                {
                    Russian = "Талисман лошади",
                })
                .WithDescription(new CustomNameInfo("Become the horse. A quest-map teleport that breaks a ban starts the 15 second countdown. Right click opens the district elevator.")
                {
                    Russian = "Стань лошадью. Телепорт с карты квестов, который обходит запрет, запускает 15 секунд. Правая кнопка открывает лифт районов.",
                })
                .WithUnlock()
                .WithSprite(CatSprites.HorseTalisman, 32f);

            RogueLibs.CreateCustomItem<TigerTalismanItem>()
                .WithName(new CustomNameInfo("Tiger Stone")
                {
                    Russian = "Камень Тигрицы",
                })
                .WithDescription(new CustomNameInfo("Become the tigress. Clout hits the creature in front for 30 and throws them through walls. The suit then lasts 15 seconds.")
                {
                    Russian = "Стань тигрицей. Удар бьёт существо впереди на 30 и швыряет его сквозь стены. После этого костюм держится 15 секунд.",
                })
                .WithUnlock()
                .WithSprite(CatSprites.TigerTalisman, 32f);

            RogueLibs.CreateCustomItem<DogTalismanItem>()
                .WithName(new CustomNameInfo("Dog Stone")
                {
                    Russian = "Камень Собаки",
                })
                .WithDescription(new CustomNameInfo("Become the dog. The ball marks instead of hurting. A successful theft from the marked person starts the 15 second countdown.")
                {
                    Russian = "Стань собакой. Мяч не ранит, а метит того, в кого попал. После удачной кражи у отмеченного костюм держится 15 секунд.",
                })
                .WithUnlock()
                .WithSprite(CatSprites.DogTalisman, 32f);

            RogueLibs.CreateCustomItem<GoatTalismanItem>()
                .WithName(new CustomNameInfo("Goat Stone")
                {
                    Russian = "Камень Козы",
                })
                .WithDescription(new CustomNameInfo("Become the goat. Genesis opens the item list and grants one wish, even a locked one. The suit then lasts 15 seconds.")
                {
                    Russian = "Стань козой. Генезис открывает список предметов и выдаёт одну желаемую вещь, даже ещё закрытую. После этого костюм держится 15 секунд.",
                })
                .WithUnlock()
                .WithSprite(CatSprites.GoatTalisman, 32f);

            RogueLibs.CreateCustomItem<RoosterTalismanItem>()
                .WithName(new CustomNameInfo("Rooster Stone")
                {
                    Russian = "Камень Петуха",
                })
                .WithDescription(new CustomNameInfo("Become the rooster. Sublimation opens the level-up trait list and grants one useful trait for one minute, and only while the suit is on.")
                {
                    Russian = "Стань петухом. Сублимация открывает список черт, как при новом уровне, и даёт одну полезную на минуту. Черта живёт только в этом костюме.",
                })
                .WithUnlock()
                .WithSprite(CatSprites.RoosterTalisman, 32f);

            RogueLibs.CreateCustomItem<DragonTalismanItem>()
                .WithName(new CustomNameInfo("Dragon Stone")
                {
                    Russian = "Камень Дракона",
                })
                .WithDescription(new CustomNameInfo("Become the dragon. Right click cycles wind, water and a taser. The first weapon starts one minute, and swapping does not reset it.")
                {
                    Russian = "Стань драконом. Правая кнопка по кругу даёт ветер, воду и тазер. С первого оружия костюм держится минуту, замена ствола время не сбрасывает.",
                })
                .WithUnlock()
                .WithSprite(CatSprites.DragonTalisman, 32f);

            RogueLibs.CreateCustomItem<OxTalismanItem>()
                .WithName(new CustomNameInfo("Ox Stone")
                {
                    Russian = "Камень Быка",
                })
                .WithDescription(new CustomNameInfo("Become the ox. Resistance clears harmful statuses like an antidote, as many times as you need. The first use starts the 15 second countdown.")
                {
                    Russian = "Стань быком. Сопротивление снимает вредные статусы, как антидот, и можно жать снова. Первое применение запускает 15 секунд.",
                })
                .WithUnlock()
                .WithSprite(CatSprites.OxTalisman, 32f);

            RogueLibs.CreateCustomItem<MonkeyTalismanItem>()
                .WithName(new CustomNameInfo("Monkey Stone")
                {
                    Russian = "Камень Обезьяны",
                })
                .WithDescription(new CustomNameInfo("Become the monkey. Uproar marks instead of hurting. Confusion flips the marked person's step for 15 seconds, and the suit then lasts 15 seconds.")
                {
                    Russian = "Стань обезьяной. Переполох не ранит, а метит того, в кого попал. Смятение на 15 секунд переворачивает ему шаг. После этого костюм держится 15 секунд.",
                })
                .WithUnlock()
                .WithSprite(CatSprites.MonkeyTalisman, 32f);

            RogueLibs.CreateCustomItem<PeacockTalismanItem>()
                .WithName(new CustomNameInfo("Peacock Stone")
                {
                    Russian = "Камень Павлина",
                })
                .WithDescription(new CustomNameInfo("Become the peacock. Amok opens the class list and calls one ally. The suit then lasts one minute. Right click again, or taking the suit off, sends the ally away.")
                {
                    Russian = "Стань павлином. Амок открывает список классов и призывает одного союзника. После этого костюм держится минуту. Повторная правая кнопка или снятие костюма отзывает союзника.",
                })
                .WithUnlock()
                .WithSprite(CatSprites.PeacockTalisman, 32f);

            RogueLibs.CreateCustomItem<FoxTalismanItem>()
                .WithName(new CustomNameInfo("Fox Stone")
                {
                    Russian = "Камень Лисы",
                })
                .WithDescription(new CustomNameInfo("Become the fox. Mirage places one hologram bigfoot, the same way the hacker's item does. The suit then lasts 15 seconds.")
                {
                    Russian = "Стань лисой. Мираж один раз ставит голограмму йети, как предмет хакера. После этого костюм держится 15 секунд.",
                })
                .WithUnlock()
                .WithSprite(CatSprites.FoxTalisman, 32f);

            RogueLibs.CreateCustomItem<PigTalismanItem>()
                .WithName(new CustomNameInfo("Pig Stone")
                {
                    Russian = "Камень Свиньи",
                })
                .WithDescription(new CustomNameInfo("Become the pig. Gift makes the person in front forget every grudge and turn Friendly to everyone. One use, then the suit lasts 15 seconds. The friendliness stays.")
                {
                    Russian = "Стань свиньёй. Подарок выбирает человека впереди: он забывает обиды и становится дружественным ко всем. Сила одна на костюм, потом форма держится 15 секунд. Дружелюбие остаётся.",
                })
                .WithUnlock()
                .WithSprite(CatSprites.PigTalisman, 32f);

            RogueLibs.CreateCustomItem<TurtleTalismanItem>()
                .WithName(new CustomNameInfo("Turtle Stone")
                {
                    Russian = "Камень Черепахи",
                })
                .WithDescription(new CustomNameInfo("Become the turtle. Right click raises a field that reflects attacks and keeps others out. Press again to lower it. The first press starts the 15 second countdown.")
                {
                    Russian = "Стань черепахой. Правая кнопка ставит поле: оно отражает атаки и не подпускает ближе. Ещё раз убирает поле. С первого нажатия костюм держится 15 секунд.",
                })
                .WithUnlock()
                .WithSprite(CatSprites.TurtleTalisman, 32f);

            RogueLibs.CreateCustomItem<ButterflyTalismanItem>()
                .WithName(new CustomNameInfo("Butterfly Stone")
                {
                    Russian = "Камень Бабочки",
                })
                .WithDescription(new CustomNameInfo("Become the butterfly. Right click turns the person in front into a random inhuman race and drives them mad. The suit then lasts one minute. Another press, or taking the suit off, restores them if they are still alive.")
                {
                    Russian = "Стань бабочкой. Правая кнопка превращает человека впереди в случайную нечеловеческую расу и вгоняет в бешенство. После этого костюм держится минуту. Повторное нажатие или снятие костюма возвращает прежний облик, если существо живо.",
                })
                .WithUnlock()
                .WithSprite(CatSprites.ButterflyTalisman, 32f);

            RogueLibs.CreateCustomItem<RabbitTalismanItem>()
                .WithName(new CustomNameInfo("Rabbit Stone")
                {
                    Russian = "Камень Кролика",
                })
                .WithDescription(new CustomNameInfo("Become the rabbit. Right click stops time for 10 seconds: creatures, cameras and traps freeze, you do not. That press starts the 15 second countdown.")
                {
                    Russian = "Стань кроликом. Правая кнопка один раз останавливает время на 10 секунд: существа, камеры и ловушки замирают, ты нет. С этого нажатия костюм держится 15 секунд.",
                })
                .WithUnlock()
                .WithSprite(CatSprites.RabbitTalisman, 32f);

            RogueLibs.CreateCustomItem<SteelStaffItem>()
                .WithName(new CustomNameInfo("Steel Staff")
                {
                    Russian = "Стальной шест",
                })
                .WithDescription(new CustomNameInfo("Cat Noir's staff. It only exists while the suit is on.")
                {
                    Russian = "Шест суперкота. Существует, только пока надет костюм.",
                })
                .WithSprite(CatSprites.Staff, 32f);

            RogueLibs.CreateCustomItem<DragonArmItem>()
                .WithName(new CustomNameInfo("Dragon Weapon")
                {
                    Russian = "Оружие дракона",
                })
                .WithDescription(new CustomNameInfo("The dragon's weapon. It only exists while the suit is on.")
                {
                    Russian = "Оружие дракона. Существует, только пока надет костюм.",
                })
                .WithSprite(CatSprites.DragonPower, 32f);

            RogueLibs.CreateCustomAbility<CataclysmAbility>()
                .WithName(new CustomNameInfo("Cataclysm")
                {
                    Russian = "Катаклизм",
                })
                .WithDescription(new CustomNameInfo("One strike. Destroys the wall in front of you, or badly wounds one creature.")
                {
                    Russian = "Один удар. Ломает стену впереди или тяжело ранит одно существо.",
                })
                .WithSprite(CatSprites.Effect, 32f);

            try
            {
                RogueLibs.CreateCustomEffect<CatNoirEffect>()
                    .WithName(new CustomNameInfo("Cat Noir")
                    {
                        Russian = "Суперкот",
                    })
                    .WithDescription(new CustomNameInfo("Black suit, steel staff, one Cataclysm.")
                    {
                        Russian = "Чёрный костюм, стальной шест, один катаклизм.",
                    })
                    .WithSprite(CatSprites.Effect, 32f);
            }
            catch (System.Exception exception)
            {
                Logger.LogWarning("Иконка эффекта суперкота не встала, форма всё равно зарегистрирована.");
                Logger.LogWarning(exception);
            }

            RogueLibs.CreateCustomItem<JumpRopeItem>()
                .WithName(new CustomNameInfo("Jump Rope")
                {
                    Russian = "Скакалка",
                })
                .WithDescription(new CustomNameInfo("The mouse's rope. It only exists while the suit is on.")
                {
                    Russian = "Скакалка мыши. Существует, только пока надет костюм.",
                })
                .WithSprite(CatSprites.Rope, 32f);

            RogueLibs.CreateCustomItem<LadybugYoyoItem>()
                .WithName(new CustomNameInfo("Ladybug Yo-yo")
                {
                    Russian = "Йо-йо Ледибаг",
                })
                .WithDescription(new CustomNameInfo("A thrown yo-yo. It always comes back.")
                {
                    Russian = "Бросковое йо-йо. Всегда возвращается в руку.",
                })
                .WithSprite(CatSprites.Yoyo, 32f);

            RogueLibs.CreateCustomItem<TrompoItem>()
                .WithName(new CustomNameInfo("Trompo")
                {
                    Russian = "Тромпо",
                })
                .WithDescription(new CustomNameInfo("The bee's spinning top. It flies out and always comes back.")
                {
                    Russian = "Волчок пчелы. Улетает и всегда возвращается в руку.",
                })
                .WithSprite(CatSprites.Trompo, 32f);

            RogueLibs.CreateCustomItem<DogBallItem>()
                .WithName(new CustomNameInfo("Ball")
                {
                    Russian = "Мяч",
                })
                .WithDescription(new CustomNameInfo("The dog's ball. It does not wound, it only marks. It only exists while the suit is on.")
                {
                    Russian = "Мяч собаки. Не ранит, только метит. Существует, только пока надет костюм.",
                })
                .WithSprite(CatSprites.Ball, 32f);

            RogueLibs.CreateCustomItem<UproarItem>()
                .WithName(new CustomNameInfo("Uproar")
                {
                    Russian = "Переполох",
                })
                .WithDescription(new CustomNameInfo("The monkey's banana. It does not wound, it only marks. It only exists while the suit is on.")
                {
                    Russian = "Банан обезьяны. Не ранит, только метит. Существует, только пока надет костюм.",
                })
                .WithSprite(CatSprites.Banana, 32f);

            RogueLibs.CreateCustomAbility<LuckyCharmAbility>()
                .WithName(new CustomNameInfo("Lucky Charm")
                {
                    Russian = "Лаки Чарм",
                })
                .WithDescription(new CustomNameInfo("One charm. Pulls a random quest reward out of thin air.")
                {
                    Russian = "Один шарм. Достаёт случайную награду из списка квестовых призов.",
                })
                .WithSprite(CatSprites.Charm, 32f);

            RogueLibs.CreateCustomAbility<MultiplyAbility>()
                .WithName(new CustomNameInfo("Multiplication")
                {
                    Russian = "Умножение",
                })
                .WithDescription(new CustomNameInfo("Become tiny and call three tiny allies for one minute.")
                {
                    Russian = "Стань маленьким и позови трёх мелких союзников на минуту.",
                })
                .WithSprite(CatSprites.Multiply, 32f);

            RogueLibs.CreateCustomAbility<VenomAbility>()
                .WithName(new CustomNameInfo("Venom")
                {
                    Russian = "Веном",
                })
                .WithDescription(new CustomNameInfo("Paralyze the creature in front of you for 15 seconds.")
                {
                    Russian = "Парализует существо впереди на 15 секунд.",
                })
                .WithSprite(CatSprites.Venom, 32f);

            RogueLibs.CreateCustomAbility<SecondChanceAbility>()
                .WithName(new CustomNameInfo("Second Chance")
                {
                    Russian = "Второй шанс",
                })
                .WithDescription(new CustomNameInfo("Mark the world. Before the 30 seconds end, use it again to return there and restart the 30 seconds.")
                {
                    Russian = "Запоминает мир. Пока не вышли 30 секунд, ещё раз возвращает к этому моменту и запускает 30 секунд заново.",
                })
                .WithSprite(CatSprites.SecondChance, 32f);

            RogueLibs.CreateCustomAbility<VoyageAbility>()
                .WithName(new CustomNameInfo("Voyage")
                {
                    Russian = "Путешествие",
                })
                .WithDescription(new CustomNameInfo("Open the district elevator. The same hero continues on the first floor of the chosen district.")
                {
                    Russian = "Открывает лифт районов. Тот же герой продолжает забег с первого этажа выбранного района.",
                })
                .WithSprite(CatSprites.Voyage, 32f);

            RogueLibs.CreateCustomAbility<CloutAbility>()
                .WithName(new CustomNameInfo("Clout")
                {
                    Russian = "Удар",
                })
                .WithDescription(new CustomNameInfo("One punch. Deals 30 damage and hurls the creature through walls.")
                {
                    Russian = "Один удар. Наносит 30 урона и пробивает противником стены.",
                })
                .WithSprite(CatSprites.Clout, 32f);

            RogueLibs.CreateCustomAbility<FetchAbility>()
                .WithName(new CustomNameInfo("Fetch")
                {
                    Russian = "Апорт",
                })
                .WithDescription(new CustomNameInfo("Steal one item from the person your ball marked, the way a thief does. An empty pocket or a full bag keeps the charge.")
                {
                    Russian = "Ворует одну вещь у человека, которого пометил мяч, как перчатка вора. Пустые карманы или полная сумка заряд не тратят.",
                })
                .WithSprite(CatSprites.Fetch, 32f);

            RogueLibs.CreateCustomAbility<GenesisAbility>()
                .WithName(new CustomNameInfo("Genesis")
                {
                    Russian = "Генезис",
                })
                .WithDescription(new CustomNameInfo("Wish for one item from the teleporter list. Closing the list without a choice keeps the charge.")
                {
                    Russian = "Загадай один предмет из списка телепортера. Закрытие списка без выбора заряд не тратит.",
                })
                .WithSprite(CatSprites.Genesis, 32f);

            RogueLibs.CreateCustomAbility<SublimationAbility>()
                .WithName(new CustomNameInfo("Sublimation")
                {
                    Russian = "Сублимация",
                })
                .WithDescription(new CustomNameInfo("Open the trait list once and take one useful trait. It lasts one minute, then the suit comes off.")
                {
                    Russian = "Один раз открывает список черт. Выбранная полезная черта держится минуту, потом костюм спадает.",
                })
                .WithSprite(CatSprites.Sublimation, 32f);

            RogueLibs.CreateCustomAbility<DragonAbility>()
                .WithName(new CustomNameInfo("Dragon")
                {
                    Russian = "Дракон",
                })
                .WithDescription(new CustomNameInfo("Cycle wind, water, then the taser. The first weapon starts one minute.")
                {
                    Russian = "По кругу: ветер, вода, тазер. Первое оружие запускает минуту.",
                })
                .WithSprite(CatSprites.DragonPower, 32f);

            RogueLibs.CreateCustomAbility<ResistanceAbility>()
                .WithName(new CustomNameInfo("Resistance")
                {
                    Russian = "Сопротивление",
                })
                .WithDescription(new CustomNameInfo("Clear the same harmful statuses as an antidote. Further presses keep working, and the first one starts 15 seconds.")
                {
                    Russian = "Снимает те же вредные статусы, что антидот. Повторные нажатия тоже работают, первое запускает 15 секунд.",
                })
                .WithSprite(CatSprites.Resistance, 32f);

            RogueLibs.CreateCustomAbility<ConfusionAbility>()
                .WithName(new CustomNameInfo("Confusion")
                {
                    Russian = "Смятение",
                })
                .WithDescription(new CustomNameInfo("Lift the Uproar mark. For 15 seconds the marked person steps backward and can barely walk. No mark keeps the charge.")
                {
                    Russian = "Снимает метку Переполоха. 15 секунд отмеченный шагает назад и едва идёт. Без метки заряд не тратится.",
                })
                .WithSprite(CatSprites.Confusion, 32f);

            RogueLibs.CreateCustomAbility<AmokAbility>()
                .WithName(new CustomNameInfo("Amok")
                {
                    Russian = "Амок",
                })
                .WithDescription(new CustomNameInfo("Open the class list once and call that person. Another press sends them away.")
                {
                    Russian = "Один раз открывает список классов и призывает выбранного человека. Повторное нажатие отзывает его.",
                })
                .WithSprite(CatSprites.Amok, 32f);

            RogueLibs.CreateCustomAbility<MirageAbility>()
                .WithName(new CustomNameInfo("Mirage")
                {
                    Russian = "Мираж",
                })
                .WithDescription(new CustomNameInfo("Place one hologram bigfoot. That press starts the 15 second countdown.")
                {
                    Russian = "Один раз ставит голограмму йети. Это нажатие запускает 15 секунд.",
                })
                .WithSprite(CatSprites.Mirage, 32f);

            RogueLibs.CreateCustomAbility<GiftAbility>()
                .WithName(new CustomNameInfo("Gift")
                {
                    Russian = "Подарок",
                })
                .WithDescription(new CustomNameInfo("The person in front forgets every grudge and turns Friendly to everyone. A miss keeps the charge. The suit then lasts 15 seconds.")
                {
                    Russian = "Человек впереди забывает обиды и становится дружественным ко всем. Промах заряд не тратит. После этого костюм держится 15 секунд.",
                })
                .WithSprite(CatSprites.Gift, 32f);

            RogueLibs.CreateCustomAbility<ShellAbility>()
                .WithName(new CustomNameInfo("Shell")
                {
                    Russian = "Панцирь",
                })
                .WithDescription(new CustomNameInfo("Raise a field that reflects attacks and keeps others out. Press again to lower it. The first press starts 15 seconds.")
                {
                    Russian = "Ставит поле, которое отражает атаки и не подпускает ближе. Ещё раз убирает поле. Первое нажатие запускает 15 секунд.",
                })
                .WithSprite(CatSprites.Shell, 32f);

            RogueLibs.CreateCustomAbility<ButterflyAbility>()
                .WithName(new CustomNameInfo("Akuma")
                {
                    Russian = "Акума",
                })
                .WithDescription(new CustomNameInfo("Once, turn the person in front into a random inhuman race and enrage them. Press again to restore their old self, if they still live.")
                {
                    Russian = "Один раз превращает человека впереди в случайную нечеловеческую расу и вгоняет в бешенство. Повторное нажатие возвращает прежний облик, если он жив.",
                })
                .WithSprite(CatSprites.Akuma, 32f);

            RogueLibs.CreateCustomAbility<TimeStopAbility>()
                .WithName(new CustomNameInfo("Time Stop")
                {
                    Russian = "Остановка времени",
                })
                .WithDescription(new CustomNameInfo("Once, freeze creatures, cameras, traps and timers for 10 seconds. You keep moving. The suit then lasts 15 seconds.")
                {
                    Russian = "Один раз замирают существа, камеры, ловушки и таймеры на 10 секунд. Ты продолжаешь идти. После этого костюм держится 15 секунд.",
                })
                .WithSprite(CatSprites.TimeStop, 32f);

            try
            {
                RogueLibs.CreateCustomEffect<LadybugEffect>()
                    .WithName(new CustomNameInfo("Ladybug")
                    {
                        Russian = "Ледибаг",
                    })
                    .WithDescription(new CustomNameInfo("Red suit, returning yo-yo, one Lucky Charm.")
                    {
                        Russian = "Красный костюм, возвращающееся йо-йо, один Лаки Чарм.",
                    })
                    .WithSprite(CatSprites.Charm, 32f);
            }
            catch (System.Exception exception)
            {
                Logger.LogWarning("Иконка эффекта Ледибаг не встала, форма всё равно зарегистрирована.");
                Logger.LogWarning(exception);
            }

            try
            {
                RogueLibs.CreateCustomEffect<MouseEffect>()
                    .WithName(new CustomNameInfo("Mouse")
                    {
                        Russian = "Мышь",
                    })
                    .WithDescription(new CustomNameInfo("Grey suit, jump rope, Multiplication.")
                    {
                        Russian = "Серый костюм, скакалка, умножение.",
                    })
                    .WithSprite(CatSprites.MouseTalisman, 32f);
            }
            catch (System.Exception exception)
            {
                Logger.LogWarning("Иконка эффекта мыши не встала, форма всё равно зарегистрирована.");
                Logger.LogWarning(exception);
            }

            try
            {
                RogueLibs.CreateCustomEffect<BeeEffect>()
                    .WithName(new CustomNameInfo("Bee")
                    {
                        Russian = "Пчела",
                    })
                    .WithDescription(new CustomNameInfo("Yellow suit, trompo, Venom.")
                    {
                        Russian = "Жёлтый костюм, тромпо, Веном.",
                    })
                    .WithSprite(CatSprites.BeeTalisman, 32f);
            }
            catch (System.Exception exception)
            {
                Logger.LogWarning("Иконка эффекта пчелы не встала, форма всё равно зарегистрирована.");
                Logger.LogWarning(exception);
            }

            try
            {
                RogueLibs.CreateCustomEffect<SnakeEffect>()
                    .WithName(new CustomNameInfo("Snake")
                    {
                        Russian = "Змея",
                    })
                    .WithDescription(new CustomNameInfo("Teal suit and Second Chance.")
                    {
                        Russian = "Бирюзовый костюм и Второй шанс.",
                    })
                    .WithSprite(CatSprites.SnakeTalisman, 32f);
            }
            catch (System.Exception exception)
            {
                Logger.LogWarning("Иконка эффекта змеи не встала, форма всё равно зарегистрирована.");
                Logger.LogWarning(exception);
            }

            try
            {
                RogueLibs.CreateCustomEffect<HorseEffect>()
                    .WithName(new CustomNameInfo("Horse")
                    {
                        Russian = "Лошадь",
                    })
                    .WithDescription(new CustomNameInfo("Brown suit. Quest teleport ignores the usual bans, and the right click opens the district elevator.")
                    {
                        Russian = "Коричневый костюм. Телепорт с карты квестов обходит обычные запреты, правая кнопка открывает лифт районов.",
                    })
                    .WithSprite(CatSprites.HorseTalisman, 32f);
            }
            catch (System.Exception exception)
            {
                Logger.LogWarning("Иконка эффекта лошади не встала, форма всё равно зарегистрирована.");
                Logger.LogWarning(exception);
            }

            try
            {
                RogueLibs.CreateCustomEffect<TigerEffect>()
                    .WithName(new CustomNameInfo("Tigress")
                    {
                        Russian = "Тигрица",
                    })
                    .WithDescription(new CustomNameInfo("Purple suit and Clout.")
                    {
                        Russian = "Фиолетовый костюм и Удар.",
                    })
                    .WithSprite(CatSprites.TigerTalisman, 32f);
            }
            catch (System.Exception exception)
            {
                Logger.LogWarning("Иконка эффекта тигрицы не встала, форма всё равно зарегистрирована.");
                Logger.LogWarning(exception);
            }

            try
            {
                RogueLibs.CreateCustomEffect<DogEffect>()
                    .WithName(new CustomNameInfo("Dog")
                    {
                        Russian = "Собака",
                    })
                    .WithDescription(new CustomNameInfo("Orange suit, ball, and Fetch.")
                    {
                        Russian = "Оранжевый костюм, мяч и Апорт.",
                    })
                    .WithSprite(CatSprites.DogTalisman, 32f);
            }
            catch (System.Exception exception)
            {
                Logger.LogWarning("Иконка эффекта собаки не встала, форма всё равно зарегистрирована.");
                Logger.LogWarning(exception);
            }

            try
            {
                RogueLibs.CreateCustomEffect<GoatEffect>()
                    .WithName(new CustomNameInfo("Goat")
                    {
                        Russian = "Коза",
                    })
                    .WithDescription(new CustomNameInfo("Grey suit and one Genesis wish.")
                    {
                        Russian = "Серый костюм и одно желание Генезиса.",
                    })
                    .WithSprite(CatSprites.GoatTalisman, 32f);
            }
            catch (System.Exception exception)
            {
                Logger.LogWarning("Иконка эффекта козы не встала, форма всё равно зарегистрирована.");
                Logger.LogWarning(exception);
            }

            try
            {
                RogueLibs.CreateCustomEffect<RoosterEffect>()
                    .WithName(new CustomNameInfo("Rooster")
                    {
                        Russian = "Петух",
                    })
                    .WithDescription(new CustomNameInfo("Orange suit and one Sublimation.")
                    {
                        Russian = "Оранжевый костюм и одна Сублимация.",
                    })
                    .WithSprite(CatSprites.RoosterTalisman, 32f);
            }
            catch (System.Exception exception)
            {
                Logger.LogWarning("Иконка эффекта петуха не встала, форма всё равно зарегистрирована.");
                Logger.LogWarning(exception);
            }

            try
            {
                RogueLibs.CreateCustomEffect<MonkeyEffect>()
                    .WithName(new CustomNameInfo("Monkey")
                    {
                        Russian = "Обезьяна",
                    })
                    .WithDescription(new CustomNameInfo("Brown suit, Uproar, and Confusion.")
                    {
                        Russian = "Коричневый костюм, Переполох и Смятение.",
                    })
                    .WithSprite(CatSprites.MonkeyTalisman, 32f);
            }
            catch (System.Exception exception)
            {
                Logger.LogWarning("Иконка эффекта обезьяны не встала, форма всё равно зарегистрирована.");
                Logger.LogWarning(exception);
            }

            try
            {
                RogueLibs.CreateCustomEffect<DragonEffect>()
                    .WithName(new CustomNameInfo("Dragon")
                    {
                        Russian = "Дракон",
                    })
                    .WithDescription(new CustomNameInfo("Red and gold suit. Right click cycles wind, water and the taser for one minute.")
                    {
                        Russian = "Красно-золотой костюм. Правая кнопка меняет ветер, воду и тазер. Минута идёт с первого оружия.",
                    })
                    .WithSprite(CatSprites.DragonTalisman, 32f);
            }
            catch (System.Exception exception)
            {
                Logger.LogWarning("Иконка эффекта дракона не встала, форма всё равно зарегистрирована.");
                Logger.LogWarning(exception);
            }

            try
            {
                RogueLibs.CreateCustomEffect<OxEffect>()
                    .WithName(new CustomNameInfo("Ox")
                    {
                        Russian = "Бык",
                    })
                    .WithDescription(new CustomNameInfo("Brown suit and Resistance.")
                    {
                        Russian = "Коричневый костюм и Сопротивление.",
                    })
                    .WithSprite(CatSprites.OxTalisman, 32f);
            }
            catch (System.Exception exception)
            {
                Logger.LogWarning("Иконка эффекта быка не встала, форма всё равно зарегистрирована.");
                Logger.LogWarning(exception);
            }

            try
            {
                RogueLibs.CreateCustomEffect<PeacockEffect>()
                    .WithName(new CustomNameInfo("Peacock")
                    {
                        Russian = "Павлин",
                    })
                    .WithDescription(new CustomNameInfo("Blue-green suit and one Amok.")
                    {
                        Russian = "Сине-зелёный костюм и один Амок.",
                    })
                    .WithSprite(CatSprites.PeacockTalisman, 32f);
            }
            catch (System.Exception exception)
            {
                Logger.LogWarning("Иконка эффекта павлина не встала, форма всё равно зарегистрирована.");
                Logger.LogWarning(exception);
            }

            try
            {
                RogueLibs.CreateCustomEffect<FoxEffect>()
                    .WithName(new CustomNameInfo("Fox")
                    {
                        Russian = "Лиса",
                    })
                    .WithDescription(new CustomNameInfo("Orange suit and one Mirage.")
                    {
                        Russian = "Оранжевый костюм и один Мираж.",
                    })
                    .WithSprite(CatSprites.FoxTalisman, 32f);
            }
            catch (System.Exception exception)
            {
                Logger.LogWarning("Иконка эффекта лисы не встала, форма всё равно зарегистрирована.");
                Logger.LogWarning(exception);
            }

            try
            {
                RogueLibs.CreateCustomEffect<PigEffect>()
                    .WithName(new CustomNameInfo("Pig")
                    {
                        Russian = "Свинья",
                    })
                    .WithDescription(new CustomNameInfo("Pink suit and one Gift.")
                    {
                        Russian = "Розовый костюм и один Подарок.",
                    })
                    .WithSprite(CatSprites.PigTalisman, 32f);
            }
            catch (System.Exception exception)
            {
                Logger.LogWarning("Иконка эффекта свиньи не встала, форма всё равно зарегистрирована.");
                Logger.LogWarning(exception);
            }

            try
            {
                RogueLibs.CreateCustomEffect<TurtleEffect>()
                    .WithName(new CustomNameInfo("Turtle")
                    {
                        Russian = "Черепаха",
                    })
                    .WithDescription(new CustomNameInfo("Green suit and a Shell.")
                    {
                        Russian = "Зелёный костюм и Панцирь.",
                    })
                    .WithSprite(CatSprites.TurtleTalisman, 32f);
            }
            catch (System.Exception exception)
            {
                Logger.LogWarning("Иконка эффекта черепахи не встала, форма всё равно зарегистрирована.");
                Logger.LogWarning(exception);
            }

            try
            {
                RogueLibs.CreateCustomEffect<ButterflyEffect>()
                    .WithName(new CustomNameInfo("Butterfly")
                    {
                        Russian = "Бабочка",
                    })
                    .WithDescription(new CustomNameInfo("Purple suit and one Akuma.")
                    {
                        Russian = "Фиолетовый костюм и одна Акума.",
                    })
                    .WithSprite(CatSprites.ButterflyTalisman, 32f);
            }
            catch (System.Exception exception)
            {
                Logger.LogWarning("Иконка эффекта бабочки не встала, форма всё равно зарегистрирована.");
                Logger.LogWarning(exception);
            }

            try
            {
                RogueLibs.CreateCustomEffect<RabbitEffect>()
                    .WithName(new CustomNameInfo("Rabbit")
                    {
                        Russian = "Кролик",
                    })
                    .WithDescription(new CustomNameInfo("White suit and one Time Stop.")
                    {
                        Russian = "Белый костюм и одна Остановка времени.",
                    })
                    .WithSprite(CatSprites.RabbitTalisman, 32f);
            }
            catch (System.Exception exception)
            {
                Logger.LogWarning("Иконка эффекта кролика не встала, форма всё равно зарегистрирована.");
                Logger.LogWarning(exception);
            }

            try
            {
                RogueLibs.CreateCustomEffect<DogMarkEffect>()
                    .WithName(new CustomNameInfo("Mark")
                    {
                        Russian = "Метка",
                    })
                    .WithDescription(new CustomNameInfo("The dog's ball marked this person. Fetch steals one item from them.")
                    {
                        Russian = "Мяч собаки пометил этого человека. Апорт заберёт у него одну вещь.",
                    })
                    .WithSprite(CatSprites.Mark, 32f);
            }
            catch (System.Exception exception)
            {
                Logger.LogWarning("Иконка метки собаки не встала, метка всё равно работает.");
                Logger.LogWarning(exception);
            }

            try
            {
                RogueLibs.CreateCustomEffect<MonkeyMarkEffect>()
                    .WithName(new CustomNameInfo("Stamp")
                    {
                        Russian = "Печать",
                    })
                    .WithDescription(new CustomNameInfo("Uproar marked this person. Confusion will turn their step around.")
                    {
                        Russian = "Переполох пометил этого человека. Смятение развернёт его шаг.",
                    })
                    .WithSprite(CatSprites.MonkeyMark, 32f);
            }
            catch (System.Exception exception)
            {
                Logger.LogWarning("Иконка печати обезьяны не встала, метка всё равно работает.");
                Logger.LogWarning(exception);
            }

            try
            {
                RogueLibs.CreateCustomEffect<MonkeyConfusionEffect>()
                    .WithName(new CustomNameInfo("Confusion")
                    {
                        Russian = "Смятение",
                    })
                    .WithDescription(new CustomNameInfo("The step goes backward and walking is hard. This lasts 15 seconds.")
                    {
                        Russian = "Шаг идёт назад, идти трудно. Так 15 секунд.",
                    })
                    .WithSprite(CatSprites.Confusion, 32f);
            }
            catch (System.Exception exception)
            {
                Logger.LogWarning("Иконка смятения не встала, смятение всё равно работает.");
                Logger.LogWarning(exception);
            }

            try
            {
                RogueLibs.CreateCustomEffect<AkumaMarkEffect>()
                    .WithName(new CustomNameInfo("Transformed")
                    {
                        Russian = "Превращение",
                    })
                    .WithDescription(new CustomNameInfo("The butterfly turned this person. Taking the charm off restores the old self, if they are still alive.")
                    {
                        Russian = "Бабочка превратила этого человека. Снятие чар вернёт прежний облик, если он ещё жив.",
                    })
                    .WithSprite(CatSprites.Akuma, 32f);
            }
            catch (System.Exception exception)
            {
                Logger.LogWarning("Иконка превращения бабочки не встала, чары всё равно работают.");
                Logger.LogWarning(exception);
            }

            new Harmony(PluginGuid).PatchAll(typeof(CatNoirPlugin).Assembly);
        }

        void Update()
        {
            TimeStop.Tick();
            GameController gc = GameController.gameController;
            if (gc == null || gc.playerAgent == null)
                return;

            RefreshRingCooldown(gc.playerAgent);
            RefreshEarringCooldown(gc.playerAgent);
            LadybugEffect.Find(gc.playerAgent)?.PaintSuit();
            MouseEffect.Maintain(gc.playerAgent);
            RefreshTalisman(gc.playerAgent, MouseTalismanItem.ItemId, MouseEffect.ReadyAt(gc.playerAgent));
            RefreshTalisman(gc.playerAgent, BeeTalismanItem.ItemId, BeeEffect.ReadyAt(gc.playerAgent));
            RefreshTalisman(gc.playerAgent, SnakeTalismanItem.ItemId, SnakeEffect.ReadyAt(gc.playerAgent));
            RefreshTalisman(gc.playerAgent, HorseTalismanItem.ItemId, HorseEffect.ReadyAt(gc.playerAgent));
            RefreshTalisman(gc.playerAgent, TigerTalismanItem.ItemId, TigerEffect.ReadyAt(gc.playerAgent));
            RefreshTalisman(gc.playerAgent, DogTalismanItem.ItemId, DogEffect.ReadyAt(gc.playerAgent));
            RefreshTalisman(gc.playerAgent, GoatTalismanItem.ItemId, GoatEffect.ReadyAt(gc.playerAgent));
            RefreshTalisman(gc.playerAgent, RoosterTalismanItem.ItemId, RoosterEffect.ReadyAt(gc.playerAgent));
            RefreshTalisman(gc.playerAgent, MonkeyTalismanItem.ItemId, MonkeyEffect.ReadyAt(gc.playerAgent));
            RefreshTalisman(gc.playerAgent, DragonTalismanItem.ItemId, DragonEffect.ReadyAt(gc.playerAgent));
            RefreshTalisman(gc.playerAgent, OxTalismanItem.ItemId, OxEffect.ReadyAt(gc.playerAgent));
            RefreshTalisman(gc.playerAgent, PeacockTalismanItem.ItemId, PeacockEffect.ReadyAt(gc.playerAgent));
            RefreshTalisman(gc.playerAgent, ButterflyTalismanItem.ItemId, ButterflyEffect.ReadyAt(gc.playerAgent));
            RefreshTalisman(gc.playerAgent, FoxTalismanItem.ItemId, FoxEffect.ReadyAt(gc.playerAgent));
            RefreshTalisman(gc.playerAgent, PigTalismanItem.ItemId, PigEffect.ReadyAt(gc.playerAgent));
            RefreshTalisman(gc.playerAgent, TurtleTalismanItem.ItemId, TurtleEffect.ReadyAt(gc.playerAgent));
            RefreshTalisman(gc.playerAgent, RabbitTalismanItem.ItemId, RabbitEffect.ReadyAt(gc.playerAgent));
            RabbitEffect.Find(gc.playerAgent)?.Maintain();
            YoyoFlight.Tick(gc);
        }

        void LateUpdate()
        {
            TurtleShell.Tick(GameController.gameController);
        }

        static void RefreshTalisman(Agent player, string itemId, float readyAt)
        {
            if (player.inventory == null)
                return;

            InvItem item = player.inventory.FindItem(itemId);
            if (item == null)
                return;

            float left = readyAt - UnityEngine.Time.time;
            if (left > 0f)
            {
                int seconds = UnityEngine.Mathf.CeilToInt(left);
                item.rechargeAmount = seconds + 1;
                item.invItemCount = item.rechargeAmount;
                item.rechargingItem = true;
                return;
            }

            if (!item.rechargingItem)
                return;

            item.rechargeAmount = 0;
            item.invItemCount = 1;
            item.rechargingItem = false;
        }

        static void RefreshRingCooldown(Agent player)
        {
            if (player.inventory == null)
                return;

            InvItem ring = player.inventory.FindItem(CatNoirRingItem.ItemId);
            if (ring == null)
                return;

            float left = CatNoirEffect.ReadyAt(player) - UnityEngine.Time.time;
            if (left > 0f)
            {
                int seconds = UnityEngine.Mathf.CeilToInt(left);
                ring.rechargeAmount = seconds + 1;
                ring.invItemCount = ring.rechargeAmount;
                ring.rechargingItem = true;
                return;
            }

            if (!ring.rechargingItem)
                return;

            ring.rechargeAmount = 0;
            ring.invItemCount = 1;
            ring.rechargingItem = false;
        }

        static void RefreshEarringCooldown(Agent player)
        {
            if (player.inventory == null)
                return;

            InvItem earrings = player.inventory.FindItem(LadybugEarringsItem.ItemId);
            if (earrings == null)
                return;

            float left = LadybugEffect.ReadyAt(player) - UnityEngine.Time.time;
            if (left > 0f)
            {
                int seconds = UnityEngine.Mathf.CeilToInt(left);
                earrings.rechargeAmount = seconds + 1;
                earrings.invItemCount = earrings.rechargeAmount;
                earrings.rechargingItem = true;
                return;
            }

            if (!earrings.rechargingItem)
                return;

            earrings.rechargeAmount = 0;
            earrings.invItemCount = 1;
            earrings.rechargingItem = false;
        }
    }
}
