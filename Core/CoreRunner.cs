using ConsoleTables;
using Core.Models;
using System.Xml;

namespace Core
{
    public class CoreRunner
    {

        public Data Data { get; set; }
        public GeneticAlgorithm GeneticAlgorithm { get; set; }


        public CoreRunner()
        {
            Data = new Data();
        }


        public void Run(int daysInWeek, int lessonsInDay, int groups, int students, int teachers, int subjects)
        {
            //GenerateData(groups, students, subjects, groups + 2);
            Data.LoadDataFromJson($"D:\\CSC\\4th\\Intelegense Systems\\Lab3 - Timetable\\Timetable\\Core", "JsonFiles");

            DisplayData();

            GeneticAlgorithm = new GeneticAlgorithm(20, Data, 1d);
            //DisplayGenerations(GeneticAlgorithm.Population);

            GeneticAlgorithm.Run(2000);
            Console.WriteLine("------------------");



            //Data.SaveDataToJson($"D:\\CSC\\4th\\Intelegense Systems\\Lab3 - Timetable\\JsonFiles");
            Data.SaveListToJson(GeneticAlgorithm.Population, Path.Combine($"D:\\CSC\\4th\\Intelegense Systems\\Lab3 - Timetable\\Timetable\\Core\\JsonFiles", "Population-random.json"));

            //DisplayGenerations(GeneticAlgorithm.Population);

            var theBest = GeneticAlgorithm.Population.OrderByDescending(t => t.Fittness).FirstOrDefault();

            DisplayGroupTimetabel(theBest);
            theBest.CalculateFitness();
            Console.WriteLine($"Fittness: {theBest.Fittness}");

            CheckForErrors(theBest);

            Data.SaveObjectToJson(theBest, Path.Combine($"D:\\CSC\\4th\\Intelegense Systems\\Lab3 - Timetable\\Timetable\\Core\\JsonFiles", "TheBest.json"));

        }
        void CheckForErrors(WeekTimeTable theBest)
        {
            //if any teacher have lesson in 2 diferent rooms
            for(int g = 0; g < theBest.GroupTimeTables.Length; g++)
            {
                for(int d = 0; d < theBest.GroupTimeTables[g].DayTimeTables.Length; d++)
                {
                    var dayTimeTable = theBest.GroupTimeTables[g].DayTimeTables[d];
                    for(int c = 0; c < dayTimeTable.TimeTableCells.Length; c++)
                    {
                        var cell = dayTimeTable.TimeTableCells[c];
                        if(cell == null) continue;

                        for(int l = c + 1; l < dayTimeTable.TimeTableCells.Length; l++)
                        {
                            var otherCell = dayTimeTable.TimeTableCells[l];
                            if(otherCell.Room == cell.Room && otherCell.Subject == cell.Subject && (!cell.IsLecture || !otherCell.IsLecture))
                            {
                                Console.WriteLine($"Error: Teacher {cell.Teacher.Name} have lesson in 2 diferent rooms at day {d}, cell {c}");
                            }
                        }
                    }
                }
            }

        }


        void DisplayGenerations(List<WeekTimeTable> generation)
        {
            var avargeFitnerss = generation.Sum(g => g.Fittness) / generation.Count;
            Console.WriteLine($"Avarege Fitness: {avargeFitnerss}");

            for(int i = 0; i < generation.Count; i++)
            {
                DisplayGeneration(generation[i]);
            }
        }


        void DisplayGeneration(WeekTimeTable table)
        {
            Console.WriteLine($"Timetable:\t Fittness: {table.Fittness}");
            Console.Write("Group\t");
            foreach(GroupTimeTable timeTable in table.GroupTimeTables)
            {
                Console.Write($"{timeTable.Group.Name}\t");
            }
            Console.WriteLine();

            for(int d = 0; d < 5; d++)
            {
                for(int c = 0; c < 4; c++)
                {
                    if(c == 0)
                    {
                        Console.Write($"Day {d}\t");
                    }
                    else
                    {
                        Console.Write("\t");
                    }

                    for(int g = 0; g < table.GroupTimeTables.Length; g++)
                    {
                        Console.Write($"{table.GroupTimeTables[g].DayTimeTables[d].TimeTableCells[c]?.Subject.Name ?? "---"}\t");
                    }
                    Console.WriteLine();
                    Console.Write("\t");

                    for(int g = 0; g < table.GroupTimeTables.Length; g++)
                    {
                        Console.Write($"{table.GroupTimeTables[g].DayTimeTables[d].TimeTableCells[c]?.Teacher.Name ?? "---"}\t");
                    }
                    Console.WriteLine();
                    Console.Write("\t");

                    for(int g = 0; g < table.GroupTimeTables.Length; g++)
                    {
                        Console.Write($"{table.GroupTimeTables[g].DayTimeTables[d].TimeTableCells[c]?.Room.Name ?? "---"}\t");
                    }
                    Console.WriteLine();
                    Console.Write("\t");

                    for(int g = 0; g < table.GroupTimeTables.Length; g++)
                    {
                        Console.Write($"{table.GroupTimeTables[g].DayTimeTables[d].TimeTableCells[c]?.IsLecture.ToString() ?? "---"}\t");
                    }
                    Console.WriteLine();
                    Console.WriteLine();
                }
                Console.WriteLine();
            }

        }

        void DisplayGroupTimetabel(WeekTimeTable table)
        {
            var consoleTable = new ConsoleTable();
            var header = new List<string>();
            header.Add("Група/День");
            foreach(GroupTimeTable timeTable in table.GroupTimeTables)
            {
                header.Add(timeTable.Group.Name);
            }

            consoleTable.AddColumn(header);
            consoleTable.Options.EnableCount = false;

            for(int d = 0; d < 5; d++)
            {
                for(int c = 0; c < 4; c++)
                {

                    var subjects = new List<string>();
                    if(c == 0)
                    {
                        subjects.Add($"День {d}");
                    }
                    else
                    {
                        subjects.Add("");
                    }

                    subjects.AddRange(table.GroupTimeTables.Select(x => x.DayTimeTables[d].TimeTableCells[c]?.Subject.Name ?? "---"));
                    consoleTable.AddRow(subjects.ToArray());
                    var teachers = new List<string>();
                    teachers.Add("");
                    teachers.AddRange(table.GroupTimeTables.Select(x => x.DayTimeTables[d].TimeTableCells[c]?.Teacher.Name ?? "---"));
                    consoleTable.AddRow(teachers.ToArray());
                    var rooms = new List<string>();
                    rooms.Add("");
                    rooms.AddRange(table.GroupTimeTables.Select(x => x.DayTimeTables[d].TimeTableCells[c]?.Room.Name ?? "---"));
                    consoleTable.AddRow(rooms.ToArray());
                    var isLectures = new List<string>();
                    isLectures.Add("");
                    isLectures.AddRange(table.GroupTimeTables.Select(x =>
                    {
                        if(x.DayTimeTables[d].TimeTableCells[c] is null)
                        {
                            return "---";
                        }
                        return x.DayTimeTables[d].TimeTableCells[c].IsLecture ? "Лекція" : "Практика";
                    }));
                    consoleTable.AddRow(isLectures.ToArray());
                    consoleTable.AddRow(Enumerable.Repeat("", table.GroupTimeTables.Count() + 1).ToArray());



                }
            }

            consoleTable.Write(Format.Alternative);

        }

        void DisplayRoomTimetabel(Data data, WeekTimeTable table)
        {
            var consoleTable = new ConsoleTable();
            var header = new List<string>();
            header.Add("Room/День");
            foreach(Room room in data.Rooms)
            {
                header.Add(room.Name);
            }

            consoleTable.AddColumn(header);
            consoleTable.Options.EnableCount = false;

            for(int d = 0; d < 5; d++)
            {
                for(int c = 0; c < 4; c++)
                {

                    var subjects = new List<string>();
                    if(c == 0)
                    {
                        subjects.Add($"День {d}");
                    }
                    else
                    {
                        subjects.Add("");
                    }



                    consoleTable.AddRow(Enumerable.Repeat("", data.Rooms.Count() + 1).ToArray());



                }
            }

            consoleTable.Write(Format.Alternative);
        }

        void DisplayTeacherTimetabel(Data data, WeekTimeTable table)
        {
            var consoleTable = new ConsoleTable();
            var header = new List<string>();
            header.Add("Викладач/День");
            foreach(Teacher teacher in data.Teachers)
            {
                header.Add(teacher.Name);
            }

            consoleTable.AddColumn(header);
            consoleTable.Options.EnableCount = false;

            for(int d = 0; d < 5; d++)
            {
                for(int c = 0; c < 4; c++)
                {
                    var teachersLessons = new List<string>();
                    if(c == 0)
                    {
                        teachersLessons.Add($"День {d}");
                    }
                    else
                    {
                        teachersLessons.Add("");
                    }
                    for(var t = 0; t < data.Teachers.Count; t++)
                    {
                        var teacher = data.Teachers[t];
                        var groups = new List<Group>();
                        foreach(var groupTimetable in table.GroupTimeTables)
                        {
                            var cell = groupTimetable.DayTimeTables[d].TimeTableCells[c];
                            if(cell.Teacher.Id == teacher.Id)
                            {
                                groups.Add(groupTimetable.Group);
                            }
                        }
                        

                    }
                }
            }

            consoleTable.Write(Format.Alternative);
        }



        private void GenerateData(int groups, int students, int subjects, int rooms)
        {
            for(int i = 0; i < groups; i++)
            {
                var group = new Group() { Id = i, Name = $"G-{i}", NumberOfStudents = Random.Shared.Next(students - (int)(students / 4), students + (int)(students / 4)) };
                Data.Groups.Add(group);
            }

            for(int i = 0; i < subjects; i++)
            {
                var subject = new Subject() { Id = i, Name = $"S-{i}", LectureHours = Random.Shared.Next(1, 5), PracticalHours = Random.Shared.Next(1, 5), RequiresSubGroupDivision = Random.Shared.Next(0, 2) == 1 };
                Data.Subjects.Add(subject);
            }

            for(int i = 0; i < 5; i++)
            {
                var teacher = new Teacher() { Id = i, Name = $"T-{i}", CanLectures = Random.Shared.Next(0, 2) == 1, CanPracticals = Random.Shared.Next(0, 2) == 1 };
                while(teacher.SubjectIds.Count == 0)
                {
                    for(int j = 0; j < Data.Subjects.Count; j++)
                    {
                        if(Random.Shared.Next(0, 2) == 1)
                        {
                            teacher.SubjectIds.Add(Data.Subjects[j].Id);
                        }
                    }
                }
                Data.Teachers.Add(teacher);
            }

            for(int i = 0; i < rooms; i++)
            {
                var room = new Room() { Id = i, Name = $"R-{i}", Capacity = Random.Shared.Next(Data.Groups.Min(x => x.NumberOfStudents), Data.Groups.Max(x => x.NumberOfStudents)) };
                Data.Rooms.Add(room);
            }

            for(int i = 0; i < Data.Groups.Count; i++)
            {
                var groupProgram = new GroupProgram() { Id = i, GroupId = Data.Groups[i].Id, SubjectIds = new List<int>() };
                while(groupProgram.SubjectIds.Count == 0)
                {
                    for(int j = 0; j < Data.Subjects.Count; j++)
                    {
                        if(Random.Shared.Next(0, 2) == 1)
                        {
                            groupProgram.SubjectIds.Add(Data.Subjects[j].Id);
                        }
                    }
                }
                Data.GroupPrograms.Add(groupProgram);
            }


        }
        void DisplayData()
        {
            Console.WriteLine("Teachers:");
            var teachersTable = new ConsoleTable();
            teachersTable.Options.NumberAlignment = Alignment.Right;
            teachersTable.AddColumn(new[] { "ID", "Name", "Subjects", "CanLectures", "CanPracticals" });
            foreach(var teacher in Data.Teachers)
            {
                var subjects = "";
                foreach(var subjectID in teacher.SubjectIds)
                {
                    var subject = Data.Subjects.FirstOrDefault(x => x.Id == subjectID);
                    subjects += $"{subject.Name}, ";
                }
                teachersTable.AddRow(teacher.Id, teacher.Name, subjects, teacher.CanLectures, teacher.CanPracticals);
            }
            teachersTable.Write(Format.Alternative);

            Console.WriteLine("\nSubjects:");
            ConsoleTable
                .From(Data.Subjects)
                .Configure(o => o.NumberAlignment = Alignment.Right)
                .Write(Format.Alternative);

            Console.WriteLine("\nGroups:");
            ConsoleTable
                .From(Data.Groups)
                .Configure(o => o.NumberAlignment = Alignment.Right)
                .Write(Format.Alternative);



            Console.WriteLine("\nGroup Programs:");
            var groupProgramsTable = new ConsoleTable();
            groupProgramsTable.Options.NumberAlignment = Alignment.Right;
            groupProgramsTable.AddColumn(new[] { "ID", "Group", "Subjects" });
            foreach(var groupProgram in Data.GroupPrograms)
            {
                var subjects = "";
                foreach(var subjectID in groupProgram.SubjectIds)
                {
                    var subject = Data.Subjects.FirstOrDefault(x => x.Id == subjectID);
                    subjects += $"{subject.Name}, ";
                }
                groupProgramsTable.AddRow(groupProgram.Id, Data.Groups.FirstOrDefault(x => x.Id == groupProgram.GroupId).Name, subjects);
            }
            groupProgramsTable.Write(Format.Alternative);

            Console.WriteLine("\nRooms:");

            ConsoleTable
                .From(Data.Rooms)
                .Configure(o => o.NumberAlignment = Alignment.Right)
                .Write(Format.Alternative);
        }
    }
}
