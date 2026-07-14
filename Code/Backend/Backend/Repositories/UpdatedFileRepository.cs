using Backend.Data;
using Backend.Models;
using Microsoft.EntityFrameworkCore;

namespace Backend.Repositories;

public class UpdatedFileRepository : IUpdatedFileRepository
{
    
    private readonly AppDbContext _context;
    public UpdatedFileRepository(AppDbContext context) => _context = context;

    public async Task<UpdatedFile?> GetByIdAsync(long updatedFileId)
        => await _context.UpdatedFiles.FindAsync(updatedFileId);

    public async Task<List<UpdatedFile>> GetByIdsAsync(IEnumerable<long> fileIds)
        => await _context.UpdatedFiles
            .Where(f => fileIds.Contains(f.FileId))
            .ToListAsync();

    public async Task<UpdatedFile?> GetActiveByIdAsync(long updatedFileId)
        => await _context.UpdatedFiles
            .Where(f => f.FileId == updatedFileId && !f.IsDeleted)
            .FirstOrDefaultAsync();

    public async Task<List<UpdatedFile>> GetAllAsync()
        => await _context.UpdatedFiles.ToListAsync();

    public async Task AddAsync(UpdatedFile updatedFile)
        => await _context.UpdatedFiles.AddAsync(updatedFile);

    public async Task AddRangeAsync(IEnumerable<UpdatedFile> files)
        => await _context.UpdatedFiles.AddRangeAsync(files);

    public void Update(UpdatedFile updatedFile)
        => _context.UpdatedFiles.Update(updatedFile);

    public void Delete(UpdatedFile updatedFile)
        => _context.UpdatedFiles.Remove(updatedFile);

    public void DeleteRange(IEnumerable<UpdatedFile> files)
        => _context.UpdatedFiles.RemoveRange(files);

    public async Task SoftDeleteAsync(long fileId)
    {
        var file = await _context.UpdatedFiles.FindAsync(fileId);
        if (file != null)
        {
            file.IsDeleted = true;
            file.DeletedTime = DateTime.Now;
        }
    }

    public async Task SaveAsync()
        => await _context.SaveChangesAsync();

}