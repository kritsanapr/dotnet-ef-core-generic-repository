//Installing EF Core tools: tool install --global dotnet-ef
//Creating migrations: dotnet ef migrations add InitialMigration
//dotnet ef dbcontext scaffold "Filename=EfCoreAcademy.db" Microsoft.EntityFrameworkCore.Sqlite
//Instead of filename you could specify the connection string if youre using another database management system (for example Oracle)

using EfCoreAcademy;
using EfCoreAcademy.Model;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

var options = new DbContextOptionsBuilder<ApplicationDbContext>().UseSqlite().Options;
var dbContext = new ApplicationDbContext(options);

dbContext.Database.Migrate();

ProcessDelete();
ProcessInsert();
ProcessSelect();
ProcessUpdate();
ProcessRepository();


void ProcessDelete()
{
    var professors = dbContext.Professors.ToList();
    var students = dbContext.Students.ToList();
    var classes = dbContext.Classes.ToList();
    var addresses = dbContext.Addresses.ToList();

    dbContext.Professors.RemoveRange(professors);
    dbContext.Students.RemoveRange(students);
    dbContext.Classes.RemoveRange(classes);
    dbContext.Addresses.RemoveRange(addresses);

    dbContext.SaveChanges();
    dbContext.Dispose();
}

void ProcessInsert()
{
    dbContext = new ApplicationDbContext(options);

    var address = new Address() { City = "Hamburg", Street = "Demostreet", Zip = "24225", HouseNumber = 1 };
    var professor = new Professor() { FirstName = "Jonathan", LastName = "Schoolman", Address = address };
    var student1 = new Student() { FirstName = "John", LastName = "Doe", Address = address };
    var student2 = new Student() { FirstName = "Maria", LastName ="Maker", Address = address };
    var class1 = new Class() { Professor = professor, Students = new List<Student> { student1, student2 }, Title = "IT" };

    dbContext.Addresses.Add(address);
    dbContext.Students.Add(student1);
    dbContext.Students.Add(student2);
    dbContext.Professors.Add(professor);
    dbContext.Classes.Add(class1);

    dbContext.SaveChanges();
    dbContext.Dispose();
}

void ProcessSelect()
{
    dbContext = new ApplicationDbContext(options);
    //var professor = dbContext.Professors.Include(p => p.Address).Single(p => p.FirstName == "Jonathan");
    var student = dbContext.Students.Include(s => s.Classes).Where(s => s.FirstName == "Maria").ToList();
    dbContext.Dispose();
}

void ProcessUpdate()
{
    dbContext = new ApplicationDbContext(options);
    //Standard behaviour of ef core is to use change tracking
    var student = dbContext.Students.Include(s => s.Classes).First();
    student.FirstName = "Tim";
    student.Classes = new List<Class>();
    dbContext.SaveChanges();

    dbContext.Dispose();
    dbContext = new ApplicationDbContext(options);

    student = dbContext.Students.AsNoTracking().Include(s => s.Classes).First();
    student.FirstName = "John";
    dbContext.SaveChanges();
    dbContext.Dispose();

    dbContext = new ApplicationDbContext(options);
    var studentUntracked = new Student() { Id = student.Id, FirstName = "Dennis", LastName = "Luckman" };
    dbContext.Students.Attach(studentUntracked);
    dbContext.Students.Entry(studentUntracked).State = EntityState.Modified;
    dbContext.SaveChanges();
    dbContext.Dispose();

    dbContext = new ApplicationDbContext(options);
    student = dbContext.Students.First();
    dbContext.Dispose();
}

async void ProcessRepository()
{
    dbContext = new ApplicationDbContext(options);
    var repository = new GenericRepository<Student>(dbContext);

    //simple select
    var students = await repository.GetAsync(null, null);
    var student = await repository.GetByIdAsync(students.First().Id);

    //Includes
    student = await repository.GetByIdAsync(student.Id, (student) => student.Address,
        (student) => student.Classes);

    //Filters
    Expression<Func<Student, bool>> filter = (student) => student.FirstName == "Maria";
    students = await repository.GetFilteredAsync(new[] { filter }, null, null);
    Console.ReadLine();
}
