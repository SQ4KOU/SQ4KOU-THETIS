from pathlib import Path
import shutil, re, subprocess

ROOT = Path(__file__).resolve().parents[2]
VENDOR = ROOT / 'Project Files' / 'lib' / 'Thetis-RADE-vendor'
CM = ROOT / 'Project Files' / 'Source' / 'ChannelMaster'
WDSP = ROOT / 'Project Files' / 'Source' / 'wdsp'
CONSOLE = ROOT / 'Project Files' / 'Source' / 'Console'
PIN = '408f2b5232ff0a2aec9b538a40d4cb1b02627b17'


def require(cond, msg):
    if not cond:
        raise RuntimeError(msg)


def replace_once(text, old, new, label):
    require(old in text, 'missing anchor: ' + label)
    return text.replace(old, new, 1)


def copy_glue():
    src = VENDOR / 'Project Files' / 'Source' / 'ChannelMaster'
    for name in ['radae.c','radae.h','radae_micdsp.c','radae_micdsp.h','r8brain_wrap.cpp','r8brain_wrap.h']:
        shutil.copyfile(src / name, CM / name)


def patch_pipe():
    p = CM / 'pipe.c'
    s = p.read_text(encoding='utf-8-sig')
    if '#include "radae.h"' not in s:
        s = replace_once(s, '#include "cmcomm.h"\n', '#include "cmcomm.h"\n#include "radae.h"\n', 'pipe include')
    if '\tcreate_radae();' not in s:
        s = replace_once(s, '\tcreate_tci();\n\tcreate_spc0();', '\tcreate_tci();\n\tcreate_radae();\n\tcreate_spc0();', 'create_radae')
    if '\tdestroy_radae();' not in s:
        s = replace_once(s, '\tdestroy_spc0();\n\tdestroy_tci();', '\tdestroy_spc0();\n\tdestroy_radae();\n\tdestroy_tci();', 'destroy_radae')
    if s.count('xradae_rx(rx, buffs[0]);') < 2:
        anchor = '\t\tcase 1: // Audio data\n\t\t\tmemcpy (ppip->rbuff[rx], buffs[0], pcm->rcvr[rx].ch_outsize * sizeof (complex));'
        repl = '\t\tcase 1: // Audio data\n\t\t\txradae_rx(rx, buffs[0]); // FreeDV RADE V1/V2 post-WDSP decode\n\t\t\tmemcpy (ppip->rbuff[rx], buffs[0], pcm->rcvr[rx].ch_outsize * sizeof (complex));'
        require(s.count(anchor) == 2, 'unexpected RX audio splice count')
        s = s.replace(anchor, repl)
    if 'xradae_tx(buff);' not in s:
        anchor = '\t\t\t}\n\t\t\txrecordwave(0, 1, 0, buff);'
        repl = '\t\t\t}\n\t\t\txradae_tx(buff); // FreeDV RADE V1/V2 pre-TXA modem injection\n\t\t\txrecordwave(0, 1, 0, buff);'
        s = replace_once(s, anchor, repl, 'TX RADE splice')
    p.write_text(s, encoding='utf-8')


def patch_dexp():
    p = WDSP / 'dexp.c'
    s = p.read_text(encoding='utf-8-sig')
    if 'void FlushDexpAudioDelay' not in s:
        anchor = '\nenum _dexpstate\n'
        fn = '''\nPORT\nvoid FlushDexpAudioDelay (int id)\n{\n\tDEXP a = pdexp[id];\n\tif (a == 0) return;\n\tEnterCriticalSection (&a->cs_update);\n\tif (a->audring != 0) flush_delring (a->audring);\n\tLeaveCriticalSection (&a->cs_update);\n}\n'''
        s = replace_once(s, anchor, fn + anchor, 'FlushDexpAudioDelay')
    p.write_text(s, encoding='utf-8')


def patch_vcxproj():
    p = CM / 'ChannelMaster.vcxproj'
    s = p.read_text(encoding='utf-8-sig')
    vbase = '../../lib/Thetis-RADE-vendor/Project Files/lib/'
    inc = ';'.join([
        vbase+'radae_c/src', vbase+'opus_dnn/include', vbase+'opus_dnn/dnn', vbase+'opus_dnn/celt',
        vbase+'r8brain', vbase+'r8brain/fft', vbase+'freedv_text/src', vbase+'freedv_text/codec2',
        vbase+'rnnoise/include', vbase+'libebur128/ebur128', vbase+'libebur128/ebur128/queue', vbase+'WebRTC_AGC'
    ])
    libbase = '$(SolutionDir)..\\lib\\Thetis-RADE-vendor\\Project Files\\lib\\'
    libdirs = ';'.join([libbase+x for x in [
        'radae_c\\build\\$(Platform)\\$(Configuration)\\',
        'opus_dnn\\build\\$(Platform)\\$(Configuration)\\',
        'rnnoise\\build\\$(Platform)\\$(Configuration)\\',
        'libebur128\\build\\$(Platform)\\$(Configuration)\\',
        'WebRTC_AGC\\build\\$(Platform)\\$(Configuration)\\']])
    deps = 'rade.lib;opus.lib;rnnoise.lib;ebur128.lib;WebRTC_AGC.lib;'

    def x64_block(m):
        b = m.group(0)
        if 'PFFFT_STATIC_DEFINE' not in b:
            b = b.replace('CHANNELMASTER_EXPORTS;%(PreprocessorDefinitions)', 'CHANNELMASTER_EXPORTS;PFFFT_STATIC_DEFINE;%(PreprocessorDefinitions)')
        if 'Thetis-RADE-vendor' not in b:
            b = re.sub(r'(<AdditionalIncludeDirectories>)(.*?)(</AdditionalIncludeDirectories>)', lambda z: z.group(1)+inc+';'+z.group(2)+z.group(3), b, count=1, flags=re.S)
            b = re.sub(r'(<AdditionalLibraryDirectories>)(.*?)(</AdditionalLibraryDirectories>)', lambda z: z.group(1)+z.group(2).rstrip(';')+';'+libdirs+';'+z.group(3), b, count=1, flags=re.S)
            b = re.sub(r'(<AdditionalDependencies>)(.*?)(</AdditionalDependencies>)', lambda z: z.group(1)+deps+z.group(2)+z.group(3), b, count=1, flags=re.S)
        return b

    pattern = r'<ItemDefinitionGroup Condition="[^\"]*(?:Debug|Release)\|x64[^\"]*">.*?</ItemDefinitionGroup>'
    s2, n = re.subn(pattern, x64_block, s, flags=re.S)
    require(n == 2, 'expected two x64 ItemDefinitionGroup blocks')
    s = s2

    if '<ClCompile Include="radae.c">' not in s:
        items = r'''  <ItemGroup>
    <ClInclude Include="radae.h" />
    <ClInclude Include="radae_micdsp.h" />
    <ClInclude Include="r8brain_wrap.h" />
    <ClCompile Include="radae.c"><ExcludedFromBuild Condition="'$(Platform)'=='Win32'">true</ExcludedFromBuild></ClCompile>
    <ClCompile Include="radae_micdsp.c"><ExcludedFromBuild Condition="'$(Platform)'=='Win32'">true</ExcludedFromBuild></ClCompile>
    <ClCompile Include="r8brain_wrap.cpp">
      <CompileAs Condition="'$(Configuration)|$(Platform)'=='Debug|x64'">CompileAsCpp</CompileAs>
      <CompileAs Condition="'$(Configuration)|$(Platform)'=='Release|x64'">CompileAsCpp</CompileAs>
      <ExcludedFromBuild Condition="'$(Platform)'=='Win32'">true</ExcludedFromBuild>
    </ClCompile>
    <ClCompile Include="..\..\lib\Thetis-RADE-vendor\Project Files\lib\r8brain\fft\pffft.c"><ExcludedFromBuild Condition="'$(Platform)'=='Win32'">true</ExcludedFromBuild></ClCompile>
    <ClCompile Include="..\..\lib\Thetis-RADE-vendor\Project Files\lib\r8brain\fft\pffft_double.c"><ExcludedFromBuild Condition="'$(Platform)'=='Win32'">true</ExcludedFromBuild></ClCompile>
    <ClCompile Include="..\..\lib\Thetis-RADE-vendor\Project Files\lib\r8brain\fft\pffft_common.c"><ExcludedFromBuild Condition="'$(Platform)'=='Win32'">true</ExcludedFromBuild></ClCompile>
    <ClCompile Include="..\..\lib\Thetis-RADE-vendor\Project Files\lib\freedv_text\src\rade_text.c"><ExcludedFromBuild Condition="'$(Platform)'=='Win32'">true</ExcludedFromBuild></ClCompile>
    <ClCompile Include="..\..\lib\Thetis-RADE-vendor\Project Files\lib\freedv_text\codec2\mpdecode_core.c"><ExcludedFromBuild Condition="'$(Platform)'=='Win32'">true</ExcludedFromBuild></ClCompile>
    <ClCompile Include="..\..\lib\Thetis-RADE-vendor\Project Files\lib\freedv_text\codec2\gp_interleaver.c"><ExcludedFromBuild Condition="'$(Platform)'=='Win32'">true</ExcludedFromBuild></ClCompile>
    <ClCompile Include="..\..\lib\Thetis-RADE-vendor\Project Files\lib\freedv_text\codec2\HRA_56_56.c"><ExcludedFromBuild Condition="'$(Platform)'=='Win32'">true</ExcludedFromBuild></ClCompile>
    <ClCompile Include="..\..\lib\Thetis-RADE-vendor\Project Files\lib\freedv_text\codec2\ldpc_codes.c"><ExcludedFromBuild Condition="'$(Platform)'=='Win32'">true</ExcludedFromBuild></ClCompile>
    <ClCompile Include="..\..\lib\Thetis-RADE-vendor\Project Files\lib\freedv_text\codec2\phi0.c"><ExcludedFromBuild Condition="'$(Platform)'=='Win32'">true</ExcludedFromBuild></ClCompile>
  </ItemGroup>
'''
        s = replace_once(s, '</Project>', items + '</Project>', 'ChannelMaster project end')
    p.write_text(s, encoding='utf-8')


def patch_csproj():
    p = CONSOLE / 'Thetis.csproj'
    s = p.read_text(encoding='utf-8-sig')

    canonical_refs = {
        '..\\Midi2Cat\\Midi2Cat.csproj': ('{66ADE184-31BA-4F5B-8007-F39EF1B90F95}', 'Midi2Cat'),
        '..\\RawInput\\RawInput.csproj': ('{4143D085-38CF-4640-BB05-2FDFFAF94D76}', 'RawInput'),
    }
    for include, (guid, name) in canonical_refs.items():
        pattern = r'<ProjectReference\s+Include="' + re.escape(include) + r'"\s*>.*?</ProjectReference>'
        repl = (f'<ProjectReference Include="{include}">\n'
                f'      <Project>{guid}</Project>\n'
                f'      <Name>{name}</Name>\n'
                f'    </ProjectReference>')
        s, n = re.subn(pattern, repl, s, count=1, flags=re.S | re.I)
        require(n == 1, 'cannot canonicalize ProjectReference: ' + name)
        require(repl in s, 'ProjectReference canonicalization verification failed: ' + name)

    if 'RadeNative.cs' not in s:
        items = '  <ItemGroup>\n    <Compile Include="RadeNative.cs" />\n    <Compile Include="RadeIntegration.cs" />\n  </ItemGroup>\n'
        s = replace_once(s, '</Project>', items + '</Project>', 'Thetis project end')
    p.write_text(s, encoding='utf-8')


def patch_console_mox_ptt():
    p = CONSOLE / 'console.cs'
    s = p.read_text(encoding='utf-8-sig')

    if 'RadeInterceptMoxChange(chkMOX.Checked)' not in s:
        anchor = '            bool bOldMox = _mox; //MW0LGE_21b used for state change delgates at end of fn\n'
        repl = ('            if (RadeInterceptMoxChange(chkMOX.Checked))\n'
                '                return;\n\n' + anchor)
        s = replace_once(s, anchor, repl, 'MOX RADE interception')

    if 'RadeAfterMoxChanged(tx);' not in s:
        anchor = '            if (bOldMox != tx) MoxChangeHandlers?.Invoke(rx2_enabled && VFOBTX ? 2 : 1, bOldMox, tx); // MW0LGE_21a\n'
        repl = ('            RadeAfterMoxChanged(tx);\n\n' + anchor)
        s = replace_once(s, anchor, repl, 'MOX RADE completed-edge hook')

    if 'RadePttStateMachine(); // SQ4KOU RADE EOO-safe arbiter' not in s:
        anchor = ('        private async void PollPTT()\n'
                  '        {\n'
                  '            while (chkPower.Checked)\n'
                  '            {\n'
                  '                int dotdashptt = NetworkIO.nativeGetDotDashPTT();\n')
        repl = ('        private async void PollPTT()\n'
                '        {\n'
                '            while (chkPower.Checked)\n'
                '            {\n'
                '                RadePttStateMachine(); // SQ4KOU RADE EOO-safe arbiter\n'
                '                if (RadePttPostReleaseBusy)\n'
                '                {\n'
                '                    await Task.Delay(1);\n'
                '                    continue;\n'
                '                }\n\n'
                '                int dotdashptt = NetworkIO.nativeGetDotDashPTT();\n')
        s = replace_once(s, anchor, repl, 'PollPTT RADE arbiter')

    p.write_text(s, encoding='utf-8')


def main():
    require(VENDOR.exists(), 'RADE vendor submodule is missing')
    try:
        head = subprocess.check_output(['git','-C',str(VENDOR),'rev-parse','HEAD'], text=True).strip()
        require(head == PIN, 'wrong vendor commit: ' + head)
    except Exception as e:
        raise RuntimeError('cannot verify vendor pin: ' + str(e))
    copy_glue()
    patch_pipe()
    patch_dexp()
    patch_vcxproj()
    patch_csproj()
    patch_console_mox_ptt()
    marker = ROOT / 'tools' / 'rade-redpitaya' / 'INTEGRATED.txt'
    marker.write_text('Thetis-RedPitaya RADE V1/V2 core integration\nVendor: sv1eia/Thetis-RADE @ '+PIN+'\nBase: e9c95220f4fab9eb829015a0a0d42dbce6fc45ac\nEOO: hardware un-key deferred until native RADE flush ack + 300 ms margin\n', encoding='utf-8')
    print('RADE integration patch applied successfully')

if __name__ == '__main__':
    main()
