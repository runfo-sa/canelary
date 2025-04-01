using Core.FileTree;

namespace Core.Services.VersionModel;

public record struct ComparasionFiles(IFile Left, IFile Right, string Id);