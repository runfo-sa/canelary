#
# Source: DotJim blog (http://dandraka.com)
# Jim Andrakakis, July 2022
#
Clear-Host
$ErrorActionPreference='Stop'

# ===== Change here =====
$listOfExtensions=@('*.yaml')
$listOfSecretNodes=@('SqlConnection')
$acceptableString='abc'
# ===== Change here =====

$codePath = (Get-Item -Path $PSScriptRoot).Parent.Parent.FullName

$errorList=New-Object -TypeName 'System.Collections.ArrayList'

foreach($ext in $listOfExtensions) {
    $list = Get-ChildItem -Path $codePath -Recurse -Filter $ext

    foreach($file in $list) {
        $fileName = $file.FullName
        if ($fileName.Contains('\bin\')) {
            continue
        }
        Write-Host "Checking $fileName for secrets"
        foreach($secretName in $listOfSecretNodes) {
			$pattern = $secretName + ': "$(acceptableString)"'
			$settings = Select-String -Path $fileName -Pattern $pattern
			if ($settings -ne $null) {
				$str = "[$fileName] contains text other than '$acceptableString', please replace this with $acceptableString before commiting."
				$errorList.Add($str) | Out-Null
				Write-Warning $str
			}
        }
    }
}

if ($errorList.Count -gt 0) {
    Write-Error 'Commit cancelled, please correct before commiting.'
}
