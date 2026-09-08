
/* ========================================================================
   ENTRY: 180001000
   NAME : FUN_180001000
   SIG  : undefined8 __fastcall FUN_180001000(undefined8 * param_1, char * param_2)
   ======================================================================== */

/* WARNING: Function: __security_check_cookie replaced with injection: security_check_cookie */

ulonglong FUN_180001000(undefined8 *param_1,char *param_2)

{
  int *_Str;
  undefined8 uVar1;
  int iVar2;
  int iVar3;
  HRESULT HVar4;
  ulonglong uVar5;
  int *piVar6;
  size_t sVar7;
  undefined4 extraout_var;
  undefined4 extraout_var_00;
  undefined4 extraout_var_01;
  int *piVar8;
  int iVar9;
  undefined1 auStackY_f8 [32];
  char local_c8;
  undefined8 local_c7;
  undefined3 uStack_bf;
  undefined5 uStack_bc;
  undefined3 uStack_b7;
  undefined8 uStack_b4;
  undefined4 local_ac;
  undefined8 local_88;
  undefined4 uStack_80;
  undefined4 uStack_7c;
  undefined4 uStack_78;
  undefined8 uStack_74;
  undefined4 local_6c;
  ulonglong local_48;
  
  uVar5 = DAT_180006000 ^ (ulonglong)auStackY_f8;
  iVar3 = *(int *)(param_1 + 1);
  iVar9 = 0;
  if (0 < iVar3) {
    piVar8 = (int *)*param_1;
    local_48 = uVar5;
    do {
      uVar5 = 0;
      piVar6 = piVar8;
      if (piVar8 != (int *)0x0) {
        do {
          if (*piVar6 == iVar9) {
            _Str = piVar6 + 0x85;
            sVar7 = strlen((char *)_Str);
            if (sVar7 < 0x20) {
              strcpy((char *)&local_88,(char *)_Str);
            }
            else {
              local_88 = *(undefined8 *)_Str;
              local_6c = 0x2e2e2e;
              uStack_74 = *(undefined8 *)(piVar6 + 0x8a);
              uStack_80 = (undefined4)*(undefined8 *)(piVar6 + 0x87);
              uStack_7c = (undefined4)*(undefined8 *)(piVar6 + 0x88);
              uStack_78 = (undefined4)((ulonglong)*(undefined8 *)(piVar6 + 0x88) >> 0x20);
            }
            iVar2 = strcmp(param_2,(char *)&local_88);
            uVar5 = CONCAT44(extraout_var,iVar2);
            if (iVar2 != 0) goto LAB_1800010a3;
            local_c8 = '\0';
            if (*(int *)((longlong)param_1 + 0x14) < 0) goto LAB_180001122;
            goto LAB_1800010c0;
          }
          piVar6 = *(int **)(piVar6 + 0xa8);
        } while (piVar6 != (int *)0x0);
        uVar5 = 0;
      }
LAB_1800010a3:
      iVar9 = iVar9 + 1;
    } while (iVar9 < iVar3);
  }
  goto LAB_180001174;
  while (piVar8 = *(int **)(piVar8 + 0xa8), piVar8 != (int *)0x0) {
LAB_1800010c0:
    if (*piVar8 == *(int *)((longlong)param_1 + 0x14)) {
      sVar7 = strlen((char *)(piVar8 + 0x85));
      if (sVar7 < 0x20) {
        strcpy(&local_c8,(char *)(piVar8 + 0x85));
      }
      else {
        local_c8 = (char)piVar8[0x85];
        local_c7 = *(undefined8 *)((longlong)piVar8 + 0x215);
        uStack_bf = (undefined3)*(undefined8 *)((longlong)piVar8 + 0x21d);
        uStack_b4 = *(undefined8 *)(piVar8 + 0x8a);
        local_ac = 0x2e2e2e;
        uStack_bc = (undefined5)*(undefined8 *)(piVar8 + 0x88);
        uStack_b7 = (undefined3)((ulonglong)*(undefined8 *)(piVar8 + 0x88) >> 0x28);
      }
      break;
    }
  }
LAB_180001122:
  uVar5 = FUN_1800011e0(param_1);
  for (piVar8 = (int *)*param_1; piVar8 != (int *)0x0; piVar8 = *(int **)(piVar8 + 0xa8)) {
    if (*piVar8 == iVar9) {
      if (*(longlong *)(piVar8 + 0xa6) == 0) {
        HVar4 = CoCreateInstance((IID *)(piVar8 + 1),(LPUNKNOWN)0x0,1,(IID *)(piVar8 + 1),
                                 (LPVOID *)&DAT_1800061a0);
        uVar1 = DAT_1800061a0;
        uVar5 = CONCAT44(extraout_var_01,HVar4);
        if (HVar4 == 0) {
          *(undefined8 *)(piVar8 + 0xa6) = DAT_1800061a0;
          *(int *)((longlong)param_1 + 0x14) = iVar9;
          return CONCAT71((int7)((ulonglong)uVar1 >> 8),1);
        }
      }
      break;
    }
  }
  DAT_1800061a0 = 0;
  if (local_c8 != '\0') {
    iVar3 = strcmp((char *)&local_88,&local_c8);
    uVar5 = CONCAT44(extraout_var_00,iVar3);
    if (iVar3 != 0) {
      uVar5 = FUN_180001000(param_1,&local_c8);
    }
  }
LAB_180001174:
  return uVar5 & 0xffffffffffffff00;
}



/* ========================================================================
   ENTRY: 1800011e0
   NAME : FUN_1800011e0
   SIG  : undefined __fastcall FUN_1800011e0(undefined8 * param_1)
   ======================================================================== */

void FUN_1800011e0(undefined8 *param_1)

{
  int *piVar1;
  
  if (*(int *)((longlong)param_1 + 0x14) == -1) {
    *(undefined4 *)((longlong)param_1 + 0x14) = 0xffffffff;
    return;
  }
  piVar1 = (int *)*param_1;
  while( true ) {
    if (piVar1 == (int *)0x0) {
      *(undefined4 *)((longlong)param_1 + 0x14) = 0xffffffff;
      return;
    }
    if (*piVar1 == *(int *)((longlong)param_1 + 0x14)) break;
    piVar1 = *(int **)(piVar1 + 0xa8);
  }
  if (*(longlong **)(piVar1 + 0xa6) != (longlong *)0x0) {
    (**(code **)(**(longlong **)(piVar1 + 0xa6) + 0x10))();
    piVar1[0xa6] = 0;
    piVar1[0xa7] = 0;
  }
  *(undefined4 *)((longlong)param_1 + 0x14) = 0xffffffff;
  return;
}



/* ========================================================================
   ENTRY: 180001260
   NAME : FUN_180001260
   SIG  : int * __fastcall FUN_180001260(HKEY param_1, LPBYTE param_2, int param_3, int * param_4)
   ======================================================================== */

/* WARNING: Function: __security_check_cookie replaced with injection: security_check_cookie */

int * FUN_180001260(HKEY param_1,LPBYTE param_2,int param_3,int *param_4)

{
  bool bVar1;
  LSTATUS LVar2;
  int iVar3;
  HFILE HVar4;
  HRESULT HVar5;
  size_t sVar6;
  int *piVar7;
  LPBYTE _Source;
  int iVar8;
  DWORD dwIndex;
  undefined1 auStackY_728 [32];
  HKEY local_6f8;
  HKEY local_6f0;
  HKEY local_6e8;
  HKEY local_6e0;
  DWORD local_6d8 [4];
  CLSID local_6c8;
  _OFSTRUCT local_6b8;
  BYTE local_628 [256];
  WCHAR local_528 [104];
  CHAR local_458 [512];
  BYTE local_258 [512];
  ulonglong local_58;
  
  local_58 = DAT_180006000 ^ (ulonglong)auStackY_728;
  if (param_4 == (int *)0x0) {
    LVar2 = RegOpenKeyExA(param_1,(LPCSTR)param_2,0,0x20019,&local_6f0);
    if (LVar2 == 0) {
      local_6d8[1] = 1;
      local_6d8[0] = 0x100;
      LVar2 = RegQueryValueExA(local_6f0,"clsid",(LPDWORD)0x0,local_6d8 + 1,local_628,local_6d8);
      if (LVar2 == 0) {
        sVar6 = strlen((char *)local_628);
        CharLowerBuffA((LPSTR)local_628,(DWORD)sVar6);
        LVar2 = RegOpenKeyA((HKEY)0xffffffff80000000,"clsid",&local_6f8);
        if (LVar2 == 0) {
          iVar8 = -1;
          bVar1 = false;
          dwIndex = 0;
          do {
            do {
              if ((bVar1) || (LVar2 = RegEnumKeyA(local_6f8,dwIndex,local_458,0x200), LVar2 != 0))
              goto LAB_1800014cf;
              dwIndex = dwIndex + 1;
              sVar6 = strlen(local_458);
              CharLowerBuffA(local_458,(DWORD)sVar6);
              iVar3 = strcmp(local_458,(char *)local_628);
            } while (iVar3 != 0);
            LVar2 = RegOpenKeyExA(local_6f8,local_458,0,0x20019,&local_6e0);
            if (LVar2 == 0) {
              LVar2 = RegOpenKeyExA(local_6e0,"InprocServer32",0,0x20019,&local_6e8);
              if (LVar2 == 0) {
                local_6d8[3] = 1;
                local_6d8[2] = 0x200;
                LVar2 = RegQueryValueExA(local_6e8,(LPCSTR)0x0,(LPDWORD)0x0,local_6d8 + 3,local_258,
                                         local_6d8 + 2);
                iVar3 = iVar8;
                if (LVar2 == 0) {
                  local_6b8.cBytes = 0x88;
                  local_6b8.szPathName[0x69] = '\0';
                  local_6b8.szPathName[0x6a] = '\0';
                  local_6b8.szPathName[0x6b] = '\0';
                  local_6b8.szPathName[0x6c] = '\0';
                  local_6b8.szPathName[0x6d] = '\0';
                  local_6b8.szPathName[0x6e] = '\0';
                  local_6b8.szPathName[0x6f] = '\0';
                  local_6b8.szPathName[0x70] = '\0';
                  local_6b8.szPathName[0x71] = '\0';
                  local_6b8.szPathName[0x72] = '\0';
                  local_6b8.szPathName[0x73] = '\0';
                  local_6b8.szPathName[0x74] = '\0';
                  local_6b8.szPathName[0x75] = '\0';
                  local_6b8.szPathName[0x76] = '\0';
                  local_6b8.szPathName[0x77] = '\0';
                  local_6b8.szPathName[0x78] = '\0';
                  local_6b8.szPathName[0x79] = '\0';
                  local_6b8.szPathName[0x7a] = '\0';
                  local_6b8.szPathName[0x7b] = '\0';
                  local_6b8.szPathName[0x7c] = '\0';
                  local_6b8.szPathName[0x7d] = '\0';
                  local_6b8.szPathName[0x7e] = '\0';
                  local_6b8.szPathName[0x7f] = '\0';
                  local_6b8.fFixedDisk = '\0';
                  local_6b8.nErrCode = 0;
                  local_6b8.Reserved1 = 0;
                  local_6b8.Reserved2 = 0;
                  local_6b8.szPathName[0] = '\0';
                  local_6b8.szPathName[1] = '\0';
                  local_6b8.szPathName[2] = '\0';
                  local_6b8.szPathName[3] = '\0';
                  local_6b8.szPathName[4] = '\0';
                  local_6b8.szPathName[5] = '\0';
                  local_6b8.szPathName[6] = '\0';
                  local_6b8.szPathName[7] = '\0';
                  local_6b8.szPathName[8] = '\0';
                  local_6b8.szPathName[9] = '\0';
                  local_6b8.szPathName[10] = '\0';
                  local_6b8.szPathName[0xb] = '\0';
                  local_6b8.szPathName[0xc] = '\0';
                  local_6b8.szPathName[0xd] = '\0';
                  local_6b8.szPathName[0xe] = '\0';
                  local_6b8.szPathName[0xf] = '\0';
                  local_6b8.szPathName[0x10] = '\0';
                  local_6b8.szPathName[0x11] = '\0';
                  local_6b8.szPathName[0x12] = '\0';
                  local_6b8.szPathName[0x13] = '\0';
                  local_6b8.szPathName[0x14] = '\0';
                  local_6b8.szPathName[0x15] = '\0';
                  local_6b8.szPathName[0x16] = '\0';
                  local_6b8.szPathName[0x17] = '\0';
                  local_6b8.szPathName[0x18] = '\0';
                  local_6b8.szPathName[0x19] = '\0';
                  local_6b8.szPathName[0x1a] = '\0';
                  local_6b8.szPathName[0x1b] = '\0';
                  local_6b8.szPathName[0x1c] = '\0';
                  local_6b8.szPathName[0x1d] = '\0';
                  local_6b8.szPathName[0x1e] = '\0';
                  local_6b8.szPathName[0x1f] = '\0';
                  local_6b8.szPathName[0x20] = '\0';
                  local_6b8.szPathName[0x21] = '\0';
                  local_6b8.szPathName[0x22] = '\0';
                  local_6b8.szPathName[0x23] = '\0';
                  local_6b8.szPathName[0x24] = '\0';
                  local_6b8.szPathName[0x25] = '\0';
                  local_6b8.szPathName[0x26] = '\0';
                  local_6b8.szPathName[0x27] = '\0';
                  local_6b8.szPathName[0x28] = '\0';
                  local_6b8.szPathName[0x29] = '\0';
                  local_6b8.szPathName[0x2a] = '\0';
                  local_6b8.szPathName[0x2b] = '\0';
                  local_6b8.szPathName[0x2c] = '\0';
                  local_6b8.szPathName[0x2d] = '\0';
                  local_6b8.szPathName[0x2e] = '\0';
                  local_6b8.szPathName[0x2f] = '\0';
                  local_6b8.szPathName[0x30] = '\0';
                  local_6b8.szPathName[0x31] = '\0';
                  local_6b8.szPathName[0x32] = '\0';
                  local_6b8.szPathName[0x33] = '\0';
                  local_6b8.szPathName[0x34] = '\0';
                  local_6b8.szPathName[0x35] = '\0';
                  local_6b8.szPathName[0x36] = '\0';
                  local_6b8.szPathName[0x37] = '\0';
                  local_6b8.szPathName[0x38] = '\0';
                  local_6b8.szPathName[0x39] = '\0';
                  local_6b8.szPathName[0x3a] = '\0';
                  local_6b8.szPathName[0x3b] = '\0';
                  local_6b8.szPathName[0x3c] = '\0';
                  local_6b8.szPathName[0x3d] = '\0';
                  local_6b8.szPathName[0x3e] = '\0';
                  local_6b8.szPathName[0x3f] = '\0';
                  local_6b8.szPathName[0x40] = '\0';
                  local_6b8.szPathName[0x41] = '\0';
                  local_6b8.szPathName[0x42] = '\0';
                  local_6b8.szPathName[0x43] = '\0';
                  local_6b8.szPathName[0x44] = '\0';
                  local_6b8.szPathName[0x45] = '\0';
                  local_6b8.szPathName[0x46] = '\0';
                  local_6b8.szPathName[0x47] = '\0';
                  local_6b8.szPathName[0x48] = '\0';
                  local_6b8.szPathName[0x49] = '\0';
                  local_6b8.szPathName[0x4a] = '\0';
                  local_6b8.szPathName[0x4b] = '\0';
                  local_6b8.szPathName[0x4c] = '\0';
                  local_6b8.szPathName[0x4d] = '\0';
                  local_6b8.szPathName[0x4e] = '\0';
                  local_6b8.szPathName[0x4f] = '\0';
                  local_6b8.szPathName[0x50] = '\0';
                  local_6b8.szPathName[0x51] = '\0';
                  local_6b8.szPathName[0x52] = '\0';
                  local_6b8.szPathName[0x53] = '\0';
                  local_6b8.szPathName[0x54] = '\0';
                  local_6b8.szPathName[0x55] = '\0';
                  local_6b8.szPathName[0x56] = '\0';
                  local_6b8.szPathName[0x57] = '\0';
                  local_6b8.szPathName[0x58] = '\0';
                  local_6b8.szPathName[0x59] = '\0';
                  local_6b8.szPathName[0x5a] = '\0';
                  local_6b8.szPathName[0x5b] = '\0';
                  local_6b8.szPathName[0x5c] = '\0';
                  local_6b8.szPathName[0x5d] = '\0';
                  local_6b8.szPathName[0x5e] = '\0';
                  local_6b8.szPathName[0x5f] = '\0';
                  local_6b8.szPathName[0x60] = '\0';
                  local_6b8.szPathName[0x61] = '\0';
                  local_6b8.szPathName[0x62] = '\0';
                  local_6b8.szPathName[99] = '\0';
                  local_6b8.szPathName[100] = '\0';
                  local_6b8.szPathName[0x65] = '\0';
                  local_6b8.szPathName[0x66] = '\0';
                  local_6b8.szPathName[0x67] = '\0';
                  local_6b8.szPathName[0x68] = '\0';
                  HVar4 = OpenFile((LPCSTR)local_258,&local_6b8,0x4000);
                  iVar3 = 0;
                  if (HVar4 == 0) {
                    iVar3 = iVar8;
                  }
                }
                iVar8 = iVar3;
                RegCloseKey(local_6e8);
              }
              RegCloseKey(local_6e0);
            }
            bVar1 = true;
          } while (LVar2 == 0);
LAB_1800014cf:
          RegCloseKey(local_6f8);
          if (iVar8 == 0) {
            param_4 = (int *)thunk_FUN_180002bd0(0x2a8);
            memset(param_4 + 1,0,0x2a4);
            *param_4 = param_3;
            MultiByteToWideChar(0,0,(LPCSTR)local_628,-1,local_528,100);
            HVar5 = CLSIDFromString(local_528,&local_6c8);
            if (HVar5 == 0) {
              param_4[1] = local_6c8.Data1;
              param_4[2] = local_6c8._4_4_;
              param_4[3] = local_6c8.Data4._0_4_;
              param_4[4] = local_6c8.Data4._4_4_;
            }
            local_6d8[1] = 1;
            local_6d8[0] = 0x100;
            LVar2 = RegQueryValueExA(local_6f0,"description",(LPDWORD)0x0,local_6d8 + 1,local_628,
                                     local_6d8);
            _Source = local_628;
            if (LVar2 != 0) {
              _Source = param_2;
            }
            strcpy((char *)(param_4 + 0x85),(char *)_Source);
          }
        }
      }
      RegCloseKey(local_6f0);
    }
  }
  else {
    piVar7 = FUN_180001260(param_1,param_2,param_3 + 1,*(int **)(param_4 + 0xa8));
    *(int **)(param_4 + 0xa8) = piVar7;
  }
  return param_4;
}



/* ========================================================================
   ENTRY: 180001610
   NAME : FUN_180001610
   SIG  : undefined8 __fastcall FUN_180001610(void)
   ======================================================================== */

undefined8 FUN_180001610(void)

{
  return 1;
}



/* ========================================================================
   ENTRY: 180001620
   NAME : FUN_180001620
   SIG  : undefined * __fastcall FUN_180001620(void)
   ======================================================================== */

undefined * FUN_180001620(void)

{
  return &DAT_1800064d0;
}



/* ========================================================================
   ENTRY: 180001630
   NAME : FUN_180001630
   SIG  : int __fastcall FUN_180001630(undefined8 param_1, undefined8 param_2, undefined8 param_3, undefined8 param_4)
   ======================================================================== */

int FUN_180001630(undefined8 param_1,undefined8 param_2,undefined8 param_3,undefined8 param_4)

{
  int iVar1;
  undefined8 *puVar2;
  undefined8 local_res20;
  
  local_res20 = param_4;
  puVar2 = (undefined8 *)FUN_180001620();
  iVar1 = __stdio_common_vsprintf_s(*puVar2,param_1,param_2,param_3,0,&local_res20);
  if (iVar1 < 0) {
    iVar1 = -1;
  }
  return iVar1;
}



/* ========================================================================
   ENTRY: 180001690
   NAME : FUN_180001690
   SIG  : undefined8 __fastcall FUN_180001690(longlong param_1)
   ======================================================================== */

/* WARNING: Function: __security_check_cookie replaced with injection: security_check_cookie */

undefined8 FUN_180001690(longlong param_1)

{
  undefined4 *puVar1;
  int iVar2;
  undefined8 uVar3;
  undefined1 auStack_e8 [32];
  undefined4 *local_c8;
  undefined4 local_c0;
  undefined4 local_b8;
  CHAR local_a8 [128];
  ulonglong local_28;
  
  local_28 = DAT_180006000 ^ (ulonglong)auStack_e8;
  if (DAT_1800061a0 == (longlong *)0x0) {
    *(undefined8 *)(param_1 + 0xac) = 0;
  }
  else {
    iVar2 = (**(code **)(*DAT_1800061a0 + 0x48))(DAT_1800061a0,param_1 + 0xac,param_1 + 0xb0);
    if (iVar2 == 0) {
      local_c8 = (undefined4 *)CONCAT44(local_c8._4_4_,*(undefined4 *)(param_1 + 0xb0));
      FUN_180001630(local_a8,0x80,"ASIOGetChannels (inputs: %d, outputs: %d);\n",
                    (ulonglong)*(uint *)(param_1 + 0xac));
      OutputDebugStringA(local_a8);
      puVar1 = (undefined4 *)(param_1 + 0xc0);
      if (DAT_1800061a0 == (longlong *)0x0) {
        *puVar1 = 0;
        *(undefined8 *)(param_1 + 0xb8) = 0;
        *(undefined4 *)(param_1 + 0xb4) = 0;
      }
      else {
        local_c8 = puVar1;
        iVar2 = (**(code **)(*DAT_1800061a0 + 0x58))
                          (DAT_1800061a0,param_1 + 0xb4,param_1 + 0xb8,param_1 + 0xbc);
        if (iVar2 == 0) {
          local_b8 = *puVar1;
          local_c0 = *(undefined4 *)(param_1 + 0xbc);
          local_c8 = (undefined4 *)CONCAT44(local_c8._4_4_,*(undefined4 *)(param_1 + 0xb8));
          FUN_180001630(local_a8,0x80,
                        "ASIOGetBufferSize (min: %d, max: %d, preferred: %d, granularity: %d);\n",
                        (ulonglong)*(uint *)(param_1 + 0xb4));
          OutputDebugStringA(local_a8);
          if ((DAT_1800061a0 == (longlong *)0x0) ||
             (iVar2 = (**(code **)(*DAT_1800061a0 + 0x68))(DAT_1800061a0,param_1 + 200), iVar2 != 0)
             ) {
            return 0xfffffffd;
          }
          uVar3 = *(undefined8 *)(param_1 + 200);
          FUN_180001630(local_a8,0x80,"ASIOGetSampleRate (sampleRate: %f);\n",uVar3);
          OutputDebugStringA(local_a8);
          if (*(double *)(param_1 + 200) != (double)*(int *)(param_1 + 0x2f0)) {
            FUN_180001630(local_a8,0x80,"Trying to change driver sample rate...",uVar3);
            OutputDebugStringA(local_a8);
            if ((DAT_1800061a0 == (longlong *)0x0) ||
               (iVar2 = (**(code **)(*DAT_1800061a0 + 0x70))
                                  (DAT_1800061a0,(double)*(int *)(param_1 + 0x2f0)), iVar2 != 0)) {
              return 0xfffffffb;
            }
            if ((DAT_1800061a0 == (longlong *)0x0) ||
               (iVar2 = (**(code **)(*DAT_1800061a0 + 0x68))(DAT_1800061a0,param_1 + 200),
               iVar2 != 0)) {
              return 0xfffffffa;
            }
            FUN_180001630(local_a8,0x80,"ASIOGetSampleRate (sampleRate: %f);\n",
                          *(undefined8 *)(param_1 + 200));
            OutputDebugStringA(local_a8);
          }
          return 0;
        }
      }
      return 0xfffffffe;
    }
  }
  return 0xffffffff;
}



/* ========================================================================
   ENTRY: 180001910
   NAME : FUN_180001910
   SIG  : undefined8 __fastcall FUN_180001910(undefined8 * param_1, int param_2)
   ======================================================================== */

/* WARNING: Globals starting with '_' overlap smaller symbols at the same address */

undefined8 FUN_180001910(undefined8 *param_1,int param_2)

{
  longlong lVar1;
  
  _DAT_180006400 = *param_1;
  uRam0000000180006408 = param_1[1];
  _DAT_180006410 = param_1[2];
  uRam0000000180006418 = param_1[3];
  _DAT_180006420 = param_1[4];
  uRam0000000180006428 = param_1[5];
  _DAT_180006430 = param_1[6];
  uRam0000000180006438 = param_1[7];
  _DAT_180006440 = param_1[8];
  uRam0000000180006448 = param_1[9];
  _DAT_180006450 = param_1[10];
  uRam0000000180006458 = param_1[0xb];
  _DAT_180006460 = param_1[0xc];
  uRam0000000180006468 = param_1[0xd];
  _DAT_180006470 = *(undefined4 *)(param_1 + 0xe);
  uRam0000000180006474 = *(undefined4 *)((longlong)param_1 + 0x74);
  uRam0000000180006478 = *(undefined4 *)(param_1 + 0xf);
  uRam000000018000647c = *(undefined4 *)((longlong)param_1 + 0x7c);
  _DAT_180006480 = *(undefined4 *)(param_1 + 0x10);
  uRam0000000180006484 = *(undefined4 *)((longlong)param_1 + 0x84);
  uRam0000000180006488 = *(undefined4 *)(param_1 + 0x11);
  uRam000000018000648c = *(undefined4 *)((longlong)param_1 + 0x8c);
  _DAT_180006490 = *(undefined4 *)(param_1 + 0x12);
  lVar1 = (longlong)param_2;
  (*DAT_1800064c8)(*(undefined8 *)(&DAT_1800062bc + lVar1 * 8),
                   *(undefined8 *)(&DAT_1800062d4 + lVar1 * 8),
                   *(undefined8 *)(&DAT_1800062ec + lVar1 * 8),
                   *(undefined8 *)(&DAT_180006304 + lVar1 * 8));
  return 0;
}



/* ========================================================================
   ENTRY: 1800019c0
   NAME : FUN_1800019c0
   SIG  : undefined __fastcall FUN_1800019c0(int param_1)
   ======================================================================== */

/* WARNING: Function: __security_check_cookie replaced with injection: security_check_cookie */
/* WARNING: Globals starting with '_' overlap smaller symbols at the same address */

void FUN_1800019c0(int param_1)

{
  int iVar1;
  longlong lVar2;
  undefined4 uVar3;
  undefined4 uVar4;
  undefined1 auStack_108 [32];
  undefined8 local_e8;
  undefined8 uStack_e0;
  undefined8 local_d8;
  undefined8 uStack_d0;
  undefined8 local_c8;
  undefined8 uStack_c0;
  undefined8 local_b8;
  undefined8 uStack_b0;
  undefined8 local_a8;
  undefined8 uStack_a0;
  undefined8 local_98;
  undefined8 uStack_90;
  undefined8 local_88;
  undefined8 uStack_80;
  undefined8 local_78;
  undefined8 uStack_70;
  undefined8 local_68;
  undefined8 uStack_60;
  undefined4 local_58;
  ulonglong local_48;
  
  local_48 = DAT_180006000 ^ (ulonglong)auStack_108;
  lVar2 = (longlong)param_1;
  local_e8._0_4_ = 0;
  local_e8._4_4_ = 0;
  uStack_e0._0_4_ = 0;
  uStack_e0._4_4_ = 0;
  local_e8 = 0;
  uStack_e0 = 0;
  local_d8._0_4_ = 0;
  local_d8._4_4_ = 0;
  uStack_d0._0_4_ = 0;
  uStack_d0._4_4_ = 0;
  local_d8 = 0;
  uStack_d0 = 0;
  local_c8._0_4_ = 0;
  local_c8._4_4_ = 0;
  uStack_c0._0_4_ = 0;
  uStack_c0._4_4_ = 0;
  local_c8 = 0;
  uStack_c0 = 0;
  uVar3 = 0;
  uStack_b0._0_4_ = 0;
  uStack_b0._4_4_ = 0;
  local_b8 = 0;
  uStack_b0 = 0;
  local_a8._0_4_ = 0;
  local_a8._4_4_ = 0;
  uStack_a0._0_4_ = 0;
  uStack_a0._4_4_ = 0;
  local_a8 = 0;
  uStack_a0 = 0;
  local_98._0_4_ = 0;
  local_98._4_4_ = 0;
  uStack_90._0_4_ = 0;
  uStack_90._4_4_ = 0;
  local_98 = 0;
  uStack_90 = 0;
  local_88._0_4_ = 0;
  local_88._4_4_ = 0;
  uStack_80._0_4_ = 0;
  uStack_80._4_4_ = 0;
  local_88 = 0;
  uStack_80 = 0;
  local_78._0_4_ = 0;
  local_78._4_4_ = 0;
  uStack_70._0_4_ = 0;
  uStack_70._4_4_ = 0;
  local_78 = 0;
  uStack_70 = 0;
  local_68._0_4_ = 0;
  local_68._4_4_ = 0;
  uStack_60._0_4_ = 0;
  uStack_60._4_4_ = 0;
  local_68 = 0;
  uStack_60 = 0;
  local_58 = 0;
  uVar4 = 0;
  if (DAT_1800061a0 != (longlong *)0x0) {
    iVar1 = (**(code **)(*DAT_1800061a0 + 0x88))(DAT_1800061a0,&local_c8,&uStack_d0);
    uVar3 = (undefined4)local_b8;
    if (iVar1 == 0) {
      uVar3 = 3;
    }
    local_b8 = CONCAT44(local_b8._4_4_,uVar3);
    uVar4 = local_b8._4_4_;
  }
  _DAT_180006400 = (undefined4)local_e8;
  uRam0000000180006404 = local_e8._4_4_;
  uRam0000000180006408 = (undefined4)uStack_e0;
  uRam000000018000640c = uStack_e0._4_4_;
  _DAT_180006410 = (undefined4)local_d8;
  uRam0000000180006414 = local_d8._4_4_;
  uRam0000000180006418 = (undefined4)uStack_d0;
  uRam000000018000641c = uStack_d0._4_4_;
  _DAT_180006420 = (undefined4)local_c8;
  uRam0000000180006424 = local_c8._4_4_;
  uRam0000000180006428 = (undefined4)uStack_c0;
  uRam000000018000642c = uStack_c0._4_4_;
  _DAT_180006430 = uVar3;
  uRam0000000180006434 = uVar4;
  uRam0000000180006438 = (undefined4)uStack_b0;
  uRam000000018000643c = uStack_b0._4_4_;
  _DAT_180006440 = (undefined4)local_a8;
  uRam0000000180006444 = local_a8._4_4_;
  uRam0000000180006448 = (undefined4)uStack_a0;
  uRam000000018000644c = uStack_a0._4_4_;
  _DAT_180006450 = (undefined4)local_98;
  uRam0000000180006454 = local_98._4_4_;
  uRam0000000180006458 = (undefined4)uStack_90;
  uRam000000018000645c = uStack_90._4_4_;
  _DAT_180006460 = (undefined4)local_88;
  uRam0000000180006464 = local_88._4_4_;
  uRam0000000180006468 = (undefined4)uStack_80;
  uRam000000018000646c = uStack_80._4_4_;
  _DAT_180006470 = (undefined4)local_78;
  uRam0000000180006474 = local_78._4_4_;
  uRam0000000180006478 = (undefined4)uStack_70;
  uRam000000018000647c = uStack_70._4_4_;
  _DAT_180006480 = (undefined4)local_68;
  uRam0000000180006484 = local_68._4_4_;
  uRam0000000180006488 = (undefined4)uStack_60;
  uRam000000018000648c = uStack_60._4_4_;
  _DAT_180006490 = local_58;
  (*DAT_1800064c8)(*(undefined8 *)(&DAT_1800062bc + lVar2 * 8),
                   *(undefined8 *)(&DAT_1800062d4 + lVar2 * 8),
                   *(undefined8 *)(&DAT_1800062ec + lVar2 * 8),
                   *(undefined8 *)(&DAT_180006304 + lVar2 * 8));
  return;
}



/* ========================================================================
   ENTRY: 180001b30
   NAME : _guard_check_icall
   SIG  : undefined __fastcall _guard_check_icall(void)
   ======================================================================== */

void _guard_check_icall(void)

{
  return;
}



/* ========================================================================
   ENTRY: 180001bb0
   NAME : FUN_180001bb0
   SIG  : int __fastcall FUN_180001bb0(longlong param_1, int param_2, int param_3, int param_4, int param_5)
   ======================================================================== */

/* WARNING: Function: __security_check_cookie replaced with injection: security_check_cookie */

int FUN_180001bb0(longlong param_1,int param_2,int param_3,int param_4,int param_5)

{
  int iVar1;
  longlong lVar2;
  undefined4 *puVar3;
  undefined4 *puVar4;
  longlong lVar5;
  int iVar6;
  int iVar7;
  undefined1 auStack_c8 [32];
  undefined *local_a8;
  CHAR local_98 [128];
  ulonglong local_18;
  
  local_18 = DAT_180006000 ^ (ulonglong)auStack_c8;
  iVar7 = *(int *)(param_1 + 0xac);
  if (((((1 < iVar7) && (iVar6 = *(int *)(param_1 + 0xb0), 1 < iVar6)) && (-1 < param_2)) &&
      ((((-1 < param_3 && (-1 < param_4)) &&
        ((-1 < param_5 && ((param_2 < iVar7 && (param_3 < iVar7)))))) && (param_4 < iVar6)))) &&
     (((param_5 < iVar6 && (param_2 != param_3)) && (param_4 != param_5)))) {
    puVar4 = (undefined4 *)(param_1 + 0xe4);
    *(undefined4 *)(param_1 + 0xdc) = 2;
    iVar6 = 0;
    puVar3 = puVar4;
    iVar7 = iVar6;
    do {
      *puVar3 = 1;
      *(undefined8 *)(puVar3 + 4) = 0;
      iVar1 = param_3;
      if (iVar7 == 0) {
        iVar1 = param_2;
      }
      *(undefined8 *)(puVar3 + 2) = 0;
      puVar3[1] = iVar1;
      iVar7 = iVar7 + 1;
      puVar3 = puVar3 + 6;
    } while (iVar7 < *(int *)(param_1 + 0xdc));
    if (1 < *(int *)(param_1 + 0xb0)) {
      *(undefined4 *)(param_1 + 0xe0) = 2;
      iVar7 = iVar6;
      do {
        *puVar3 = 0;
        *(undefined8 *)(puVar3 + 4) = 0;
        *(undefined8 *)(puVar3 + 2) = 0;
        iVar1 = param_5;
        if (iVar7 == 0) {
          iVar1 = param_4;
        }
        iVar7 = iVar7 + 1;
        puVar3[1] = iVar1;
        puVar3 = puVar3 + 6;
      } while (iVar7 < *(int *)(param_1 + 0xe0));
      iVar7 = *(int *)(param_1 + 0xdc) + *(int *)(param_1 + 0xe0);
      if (DAT_1800061a0 == (longlong *)0x0) {
        if (0 < iVar7) {
          do {
            *(undefined8 *)(puVar4 + 4) = 0;
            *(undefined8 *)(puVar4 + 2) = 0;
            puVar4 = puVar4 + 6;
            iVar7 = iVar7 + -1;
          } while (iVar7 != 0);
        }
        return -1000;
      }
      local_a8 = &DAT_1800061b0;
      iVar7 = (**(code **)(*DAT_1800061a0 + 0x98))();
      if (iVar7 != 0) {
        return iVar7;
      }
      if (0 < *(int *)(param_1 + 0xe0) + *(int *)(param_1 + 0xdc)) {
        do {
          lVar2 = (longlong)iVar6;
          lVar5 = lVar2 * 0x34;
          *(undefined4 *)(param_1 + 0x144 + lVar5) = *(undefined4 *)(param_1 + 0xe8 + lVar2 * 0x18);
          *(undefined4 *)(param_1 + 0x148 + lVar5) = *(undefined4 *)(param_1 + 0xe4 + lVar2 * 0x18);
          if (DAT_1800061a0 == (longlong *)0x0) {
            *(undefined4 *)(param_1 + 0x158 + lVar5) = DAT_1800042d8;
            *(undefined1 *)(param_1 + 0x15c + lVar5) = DAT_1800042dc;
            *(undefined4 *)(param_1 + 0x150 + lVar5) = 0xffffffff;
            *(undefined4 *)(param_1 + 0x154 + lVar5) = 0;
            return -1000;
          }
          iVar7 = (**(code **)(*DAT_1800061a0 + 0x90))(DAT_1800061a0,lVar5 + 0x144 + param_1);
          if (iVar7 != 0) {
            return iVar7;
          }
          iVar6 = iVar6 + 1;
        } while (iVar6 < *(int *)(param_1 + 0xe0) + *(int *)(param_1 + 0xdc));
      }
      if (DAT_1800061a0 == (longlong *)0x0) {
        *(undefined8 *)(param_1 + 0xd4) = 0;
        return -1000;
      }
      iVar7 = (**(code **)(*DAT_1800061a0 + 0x50))(DAT_1800061a0,param_1 + 0xd4,param_1 + 0xd8);
      if (iVar7 != 0) {
        return iVar7;
      }
      local_a8 = (undefined *)CONCAT44(local_a8._4_4_,*(undefined4 *)(param_1 + 0xd8));
      FUN_180001630(local_98,0x80,"ASIOGetLatencies (input: %d, output: %d);\n",
                    (ulonglong)*(uint *)(param_1 + 0xd4));
      OutputDebugStringA(local_98);
      return 0;
    }
  }
  return -0x3e5;
}



/* ========================================================================
   ENTRY: 180001ed0
   NAME : prepareASIO
   SIG  : undefined8 __fastcall prepareASIO(undefined4 param_1, undefined4 param_2, undefined8 * param_3, undefined8 param_4, int param_5, uint param_6)
   ======================================================================== */

/* WARNING: Function: __security_check_cookie replaced with injection: security_check_cookie */
/* WARNING: Globals starting with '_' overlap smaller symbols at the same address */

undefined8
prepareASIO(undefined4 param_1,undefined4 param_2,undefined8 *param_3,undefined8 param_4,int param_5
           ,uint param_6)

{
  LSTATUS LVar1;
  undefined4 uVar2;
  uint uVar3;
  longlong *plVar4;
  int *piVar5;
  longlong lVar6;
  undefined8 uVar7;
  int iVar8;
  ulonglong uVar9;
  ulonglong uVar10;
  undefined1 auStackY_198 [32];
  HKEY local_158 [2];
  CHAR local_148 [128];
  BYTE local_c8 [128];
  ulonglong local_48;
  
                    /* 0x1ed0  9  prepareASIO */
  local_48 = DAT_180006000 ^ (ulonglong)auStackY_198;
  uVar9 = 0;
  _DAT_180006499 = *param_3;
  uRam00000001800064a1 = param_3[1];
  _DAT_1800064a9 = param_3[2];
  uRam00000001800064b1 = param_3[3];
  _DAT_1800064bc = param_1;
  _DAT_1800064c0 = param_2;
  DAT_1800064c8 = param_4;
  if (DAT_1800061a8 == (longlong *)0x0) {
    plVar4 = (longlong *)FUN_180002bd0(0x18);
    *(undefined8 *)((longlong)plVar4 + 0xc) = 0;
    *(undefined4 *)((longlong)plVar4 + 0x14) = 0;
    local_158[0] = (HKEY)0x0;
    *(undefined4 *)(plVar4 + 1) = 0;
    *plVar4 = 0;
    LVar1 = RegOpenKeyA((HKEY)0xffffffff80000002,"software\\asio",local_158);
    uVar10 = uVar9;
    if (LVar1 == 0) {
      while( true ) {
        param_4 = 0x80;
        LVar1 = RegEnumKeyA(local_158[0],(DWORD)uVar10,(LPSTR)local_c8,0x80);
        if (LVar1 != 0) break;
        uVar10 = (ulonglong)((DWORD)uVar10 + 1);
        piVar5 = FUN_180001260(local_158[0],local_c8,0,(int *)*plVar4);
        *plVar4 = (longlong)piVar5;
      }
    }
    if (local_158[0] != (HKEY)0x0) {
      RegCloseKey(local_158[0]);
    }
    lVar6 = *plVar4;
    if (lVar6 != 0) {
      iVar8 = (int)plVar4[1];
      do {
        iVar8 = iVar8 + 1;
        *(int *)(plVar4 + 1) = iVar8;
        lVar6 = *(longlong *)(lVar6 + 0x2a0);
      } while (lVar6 != 0);
    }
    if ((int)plVar4[1] != 0) {
      CoInitialize((LPVOID)0x0);
    }
    *(undefined4 *)((longlong)plVar4 + 0x14) = 0xffffffff;
    DAT_1800061a8 = plVar4;
  }
  uVar7 = FUN_180001000(DAT_1800061a8,&DAT_180006499);
  if ((char)uVar7 == '\0') {
    FUN_180001630(local_148,0x80,"loadAsioDriver() FAILED\n",param_4);
    OutputDebugStringA(local_148);
    return 1;
  }
  FUN_180001630(local_148,0x80,"loadAsioDriver() OK\n",param_4);
  OutputDebugStringA(local_148);
  DAT_1800061e0 = s_No_ASIO_Driver_1800042b0[8];
  DAT_1800061e0_1._0_1_ = s_No_ASIO_Driver_1800042b0[9];
  DAT_1800061e0_1._1_1_ = s_No_ASIO_Driver_1800042b0[10];
  DAT_1800061e0_1._2_1_ = s_No_ASIO_Driver_1800042b0[0xb];
  DAT_1800061e4 = s_No_ASIO_Driver_1800042b0[0xc];
  register0x00000001 = s_No_ASIO_Driver_1800042b0[0xd];
  DAT_1800061e6 = s_No_ASIO_Driver_1800042b0[0xe];
  _DAT_1800061d0 = 2;
  DAT_1800061d8 = s_No_ASIO_Driver_1800042b0[0];
  DAT_1800061d8_1._0_1_ = s_No_ASIO_Driver_1800042b0[1];
  DAT_1800061d8_1._1_1_ = s_No_ASIO_Driver_1800042b0[2];
  DAT_1800061d8_1._2_1_ = s_No_ASIO_Driver_1800042b0[3];
  DAT_1800061d8_1._3_1_ = s_No_ASIO_Driver_1800042b0[4];
  DAT_1800061d8_1._4_1_ = s_No_ASIO_Driver_1800042b0[5];
  DAT_1800061d8_1._5_1_ = s_No_ASIO_Driver_1800042b0[6];
  DAT_1800061d8_1._6_1_ = s_No_ASIO_Driver_1800042b0[7];
  if (DAT_1800061a0 != (longlong *)0x0) {
    iVar8 = (**(code **)(*DAT_1800061a0 + 0x18))(DAT_1800061a0,DAT_180006274);
    if (iVar8 != 0) {
      DAT_1800061f8 = s_No_ASIO_Driver_Error_1800042c0[0];
      DAT_1800061f8_1._0_1_ = s_No_ASIO_Driver_Error_1800042c0[1];
      DAT_1800061f8_1._1_1_ = s_No_ASIO_Driver_Error_1800042c0[2];
      DAT_1800061f8_1._2_1_ = s_No_ASIO_Driver_Error_1800042c0[3];
      uRam00000001800061fc._0_1_ = s_No_ASIO_Driver_Error_1800042c0[4];
      uRam00000001800061fc._1_1_ = s_No_ASIO_Driver_Error_1800042c0[5];
      uRam00000001800061fc._2_1_ = s_No_ASIO_Driver_Error_1800042c0[6];
      uRam00000001800061fc._3_1_ = s_No_ASIO_Driver_Error_1800042c0[7];
      uRam0000000180006200._0_1_ = s_No_ASIO_Driver_Error_1800042c0[8];
      uRam0000000180006200._1_1_ = s_No_ASIO_Driver_Error_1800042c0[9];
      uRam0000000180006200._2_1_ = s_No_ASIO_Driver_Error_1800042c0[10];
      uRam0000000180006200._3_1_ = s_No_ASIO_Driver_Error_1800042c0[0xb];
      uRam0000000180006204._0_1_ = s_No_ASIO_Driver_Error_1800042c0[0xc];
      DAT_180006205 = s_No_ASIO_Driver_Error_1800042c0[0xd];
      uRam0000000180006204._2_1_ = s_No_ASIO_Driver_Error_1800042c0[0xe];
      uRam0000000180006204._3_1_ = s_No_ASIO_Driver_Error_1800042c0[0xf];
      uRam0000000180006208._0_1_ = s_No_ASIO_Driver_Error_1800042c0[0x10];
      uRam0000000180006208._1_1_ = s_No_ASIO_Driver_Error_1800042c0[0x11];
      uRam0000000180006208._2_1_ = s_No_ASIO_Driver_Error_1800042c0[0x12];
      uRam0000000180006208._3_1_ = s_No_ASIO_Driver_Error_1800042c0[0x13];
      uRam0000000180006208._4_1_ = s_No_ASIO_Driver_Error_1800042c0[0x14];
      (**(code **)(*DAT_1800061a0 + 0x20))(DAT_1800061a0,&DAT_1800061d8);
      uVar2 = (**(code **)(*DAT_1800061a0 + 0x28))();
      _DAT_1800061d0 = CONCAT44(uVar2,DAT_1800061d0);
      FUN_180001630(local_148,0x80,"ASIOInit() OK\n",param_4);
      OutputDebugStringA(local_148);
      uVar10 = _DAT_1800061d0 & 0xffffffff;
      FUN_180001630(local_148,0x80,
                    "asioVersion:   %d\ndriverVersion: %d\nName:          %s\nErrorMessage:  %s\n",
                    uVar10);
      OutputDebugStringA(local_148);
      uVar7 = FUN_180001690(0x1800061d0);
      if ((int)uVar7 == 0) {
        FUN_180001630(local_148,0x80,"init_asio_static_data() OK\n",uVar10);
        OutputDebugStringA(local_148);
        uVar10 = (ulonglong)param_6;
        _DAT_1800061b0 = FUN_1800019c0;
        _DAT_1800061b8 = _guard_check_icall;
        _DAT_1800061c0 = &LAB_180001b40;
        _DAT_1800061c8 = FUN_180001910;
        uVar3 = FUN_180001bb0(0x1800061d0,param_5,param_5 + 1,param_6,param_6 + 1);
        if (uVar3 == 0) {
          FUN_180001630(local_148,0x80,"create_asio_buffers() OK\n\n",uVar10);
          OutputDebugStringA(local_148);
          iVar8 = 0;
          if (0 < DAT_1800062b0 + _DAT_1800062ac) {
            do {
              if ((&DAT_180006324)[(longlong)(int)uVar9 * 0xd] != 0x12) {
                iVar8 = -0x3e5;
              }
              FUN_180001630(local_148,0x80,
                            "Channel %d:  type = %d   name = %s   isInput = %d   channelNum = %d",
                            uVar9);
              OutputDebugStringA(local_148);
              uVar3 = (int)uVar9 + 1;
              uVar9 = (ulonglong)uVar3;
            } while ((int)uVar3 < DAT_1800062b0 + _DAT_1800062ac);
          }
          if (DAT_1800061a0 == (longlong *)0x0) {
            _DAT_18000628c = 0;
            _DAT_180006284 = 0;
          }
          else {
            (**(code **)(*DAT_1800061a0 + 0x58))
                      (DAT_1800061a0,&DAT_180006284,&DAT_180006288,&DAT_18000628c);
            if (DAT_1800061a0 != (longlong *)0x0) {
              (**(code **)(*DAT_1800061a0 + 0x68))(DAT_1800061a0,&DAT_180006298);
            }
          }
          FUN_180001630(local_148,0x80,"\nSample Rate = %f\nBuffer Size = %d",DAT_180006298);
          OutputDebugStringA(local_148);
          if (iVar8 == -0x3e5) {
            FUN_180001630(local_148,0x80,"Incompatible sample format type: %d",
                          (ulonglong)DAT_180006324);
            OutputDebugStringA(local_148);
            return 0xfffffc1b;
          }
          return 0;
        }
        FUN_180001630(local_148,0x80,"create_asio_buffers() FAILED : ASIOError = %d\n\n",
                      (ulonglong)uVar3);
        OutputDebugStringA(local_148);
      }
      plVar4 = DAT_1800061a8;
      if (DAT_1800061a0 != (longlong *)0x0) {
        if ((*(int *)((longlong)DAT_1800061a8 + 0x14) != -1) &&
           (piVar5 = (int *)*DAT_1800061a8, piVar5 != (int *)0x0)) {
          do {
            if (*piVar5 == *(int *)((longlong)DAT_1800061a8 + 0x14)) {
              if (*(longlong **)(piVar5 + 0xa6) != (longlong *)0x0) {
                (**(code **)(**(longlong **)(piVar5 + 0xa6) + 0x10))();
                piVar5[0xa6] = 0;
                piVar5[0xa7] = 0;
              }
              goto LAB_180002483;
            }
            piVar5 = *(int **)(piVar5 + 0xa8);
          } while (piVar5 != (int *)0x0);
          *(undefined4 *)((longlong)DAT_1800061a8 + 0x14) = 0xffffffff;
          DAT_1800061a0 = (longlong *)0x0;
          goto LAB_1800020c8;
        }
LAB_180002483:
        *(undefined4 *)((longlong)plVar4 + 0x14) = 0xffffffff;
      }
      DAT_1800061a0 = (longlong *)0x0;
      goto LAB_1800020c8;
    }
    (**(code **)(*DAT_1800061a0 + 0x30))(DAT_1800061a0,&DAT_1800061f8);
    DAT_1800061a0 = (longlong *)0x0;
  }
  FUN_180001630(local_148,0x80,"ASIOInit() FAILED\n",param_4);
  OutputDebugStringA(local_148);
LAB_1800020c8:
  plVar4 = DAT_1800061a8;
  if ((*(int *)((longlong)DAT_1800061a8 + 0x14) == -1) ||
     (piVar5 = (int *)*DAT_1800061a8, piVar5 == (int *)0x0)) {
LAB_1800024af:
    *(undefined4 *)((longlong)plVar4 + 0x14) = 0xffffffff;
  }
  else {
    do {
      if (*piVar5 == *(int *)((longlong)DAT_1800061a8 + 0x14)) {
        if (*(longlong **)(piVar5 + 0xa6) != (longlong *)0x0) {
          (**(code **)(**(longlong **)(piVar5 + 0xa6) + 0x10))();
          piVar5[0xa6] = 0;
          piVar5[0xa7] = 0;
        }
        goto LAB_1800024af;
      }
      piVar5 = *(int **)(piVar5 + 0xa8);
    } while (piVar5 != (int *)0x0);
    *(undefined4 *)((longlong)DAT_1800061a8 + 0x14) = 0xffffffff;
  }
  return 1;
}



/* ========================================================================
   ENTRY: 1800024f0
   NAME : unloadASIO
   SIG  : undefined __fastcall unloadASIO(void)
   ======================================================================== */

/* WARNING: Function: __security_check_cookie replaced with injection: security_check_cookie */

void unloadASIO(void)

{
  uint uVar1;
  undefined1 auStack_c8 [32];
  undefined4 local_a8;
  CHAR local_98 [128];
  ulonglong local_18;
  
                    /* 0x24f0  10  unloadASIO */
  local_18 = DAT_180006000 ^ (ulonglong)auStack_c8;
  if (DAT_1800061a0 == (longlong *)0x0) {
    uVar1 = 0xfffffc18;
  }
  else {
    uVar1 = (**(code **)(*DAT_1800061a0 + 0xa0))();
    if (DAT_1800061a0 != (longlong *)0x0) {
      FUN_1800011e0(DAT_1800061a8);
    }
  }
  DAT_1800061a0 = (longlong *)0x0;
  FUN_1800011e0(DAT_1800061a8);
  local_a8 = 0;
  FUN_180001630(local_98,0x80,"unloadASIO results: %d : %d",(ulonglong)uVar1);
  OutputDebugStringA(local_98);
  return;
}



/* ========================================================================
   ENTRY: 1800025a0
   NAME : getASIODriverString
   SIG  : uint __fastcall getASIODriverString(char * param_1)
   ======================================================================== */

/* WARNING: Function: __security_check_cookie replaced with injection: security_check_cookie */

uint getASIODriverString(char *param_1)

{
  uint uVar1;
  undefined1 auStackY_e8 [32];
  DWORD local_a8 [4];
  CHAR local_98 [128];
  ulonglong local_18;
  
                    /* 0x25a0  7  getASIODriverString */
  local_18 = DAT_180006000 ^ (ulonglong)auStackY_e8;
  local_a8[0] = 0x20;
  uVar1 = RegGetValueA((HKEY)0xffffffff80000001,"SOFTWARE\\OpenHPSDR\\Thetis-x64","ASIOdrivername",
                       0x10002,(LPDWORD)0x0,param_1,local_a8);
  if (*param_1 == '\0') {
    uVar1 = RegGetValueA((HKEY)0xffffffff80000002,"SOFTWARE\\OpenHPSDR\\Thetis-x64","ASIOdrivername"
                         ,0x10002,(LPDWORD)0x0,param_1,local_a8);
  }
  FUN_180001630(local_98,0x80,"RegGetValue(sz) status = %d",(ulonglong)uVar1);
  OutputDebugStringA(local_98);
  if (*param_1 == '\0') {
    uVar1 = 2;
  }
  return uVar1;
}



/* ========================================================================
   ENTRY: 1800026a0
   NAME : getASIOBlockNum
   SIG  : uint __fastcall getASIOBlockNum(PVOID param_1)
   ======================================================================== */

/* WARNING: Function: __security_check_cookie replaced with injection: security_check_cookie */

uint getASIOBlockNum(PVOID param_1)

{
  uint uVar1;
  undefined1 auStackY_e8 [32];
  DWORD local_a8 [4];
  CHAR local_98 [128];
  ulonglong local_18;
  
                    /* 0x26a0  6  getASIOBlockNum */
  local_18 = DAT_180006000 ^ (ulonglong)auStackY_e8;
  local_a8[0] = 4;
  uVar1 = RegGetValueA((HKEY)0xffffffff80000001,"SOFTWARE\\OpenHPSDR\\Thetis-x64","ASIOblocknum",
                       0x10010,(LPDWORD)0x0,param_1,local_a8);
  if (uVar1 == 2) {
    uVar1 = RegGetValueA((HKEY)0xffffffff80000002,"SOFTWARE\\OpenHPSDR\\Thetis-x64","ASIOblocknum",
                         0x10010,(LPDWORD)0x0,param_1,local_a8);
  }
  FUN_180001630(local_98,0x80,"RegGetValue(dword) status = %d",(ulonglong)uVar1);
  OutputDebugStringA(local_98);
  return uVar1;
}



/* ========================================================================
   ENTRY: 180002790
   NAME : asioStart
   SIG  : uint __fastcall asioStart(void)
   ======================================================================== */

/* WARNING: Function: __security_check_cookie replaced with injection: security_check_cookie */

uint asioStart(void)

{
  uint uVar1;
  undefined8 in_R9;
  undefined1 auStack_b8 [32];
  CHAR local_98 [128];
  ulonglong local_18;
  
                    /* 0x2790  2  asioStart */
  local_18 = DAT_180006000 ^ (ulonglong)auStack_b8;
  if (DAT_1800061a0 == (longlong *)0x0) {
    uVar1 = 0xfffffc18;
  }
  else {
    uVar1 = (**(code **)(*DAT_1800061a0 + 0x38))();
    if (uVar1 == 0) {
      FUN_180001630(local_98,0x80,"ASIOStart OK",in_R9);
      goto LAB_1800027d5;
    }
  }
  FUN_180001630(local_98,0x80,"ASIOStart Failed = %d",(ulonglong)uVar1);
LAB_1800027d5:
  OutputDebugStringA(local_98);
  return uVar1;
}



/* ========================================================================
   ENTRY: 180002820
   NAME : asioStop
   SIG  : uint __fastcall asioStop(void)
   ======================================================================== */

/* WARNING: Function: __security_check_cookie replaced with injection: security_check_cookie */

uint asioStop(void)

{
  uint uVar1;
  undefined8 in_R9;
  undefined1 auStack_b8 [32];
  CHAR local_98 [128];
  ulonglong local_18;
  
                    /* 0x2820  3  asioStop */
  local_18 = DAT_180006000 ^ (ulonglong)auStack_b8;
  if (DAT_1800061a0 == (longlong *)0x0) {
    uVar1 = 0xfffffc18;
  }
  else {
    uVar1 = (**(code **)(*DAT_1800061a0 + 0x40))();
    if (uVar1 == 0) {
      FUN_180001630(local_98,0x80,"ASIOStop OK",in_R9);
      goto LAB_180002865;
    }
  }
  FUN_180001630(local_98,0x80,"ASIOStop Failed = %d",(ulonglong)uVar1);
LAB_180002865:
  OutputDebugStringA(local_98);
  return uVar1;
}



/* ========================================================================
   ENTRY: 1800028b0
   NAME : getASIOBaseInputChannel
   SIG  : uint __fastcall getASIOBaseInputChannel(PVOID param_1)
   ======================================================================== */

/* WARNING: Function: __security_check_cookie replaced with injection: security_check_cookie */

uint getASIOBaseInputChannel(PVOID param_1)

{
  uint uVar1;
  undefined1 auStackY_e8 [32];
  DWORD local_a8 [4];
  CHAR local_98 [128];
  ulonglong local_18;
  
                    /* 0x28b0  4  getASIOBaseInputChannel */
  local_18 = DAT_180006000 ^ (ulonglong)auStackY_e8;
  local_a8[0] = 4;
  uVar1 = RegGetValueA((HKEY)0xffffffff80000001,"SOFTWARE\\OpenHPSDR\\Thetis-x64",
                       "ASIObaseinchannel",0x10010,(LPDWORD)0x0,param_1,local_a8);
  if (uVar1 == 2) {
    uVar1 = RegGetValueA((HKEY)0xffffffff80000002,"SOFTWARE\\OpenHPSDR\\Thetis-x64",
                         "ASIObaseinchannel",0x10010,(LPDWORD)0x0,param_1,local_a8);
  }
  FUN_180001630(local_98,0x80,"RegGetValue(dword) status = %d",(ulonglong)uVar1);
  OutputDebugStringA(local_98);
  return uVar1;
}



/* ========================================================================
   ENTRY: 1800029a0
   NAME : getASIOBaseOutputChannel
   SIG  : uint __fastcall getASIOBaseOutputChannel(PVOID param_1)
   ======================================================================== */

/* WARNING: Function: __security_check_cookie replaced with injection: security_check_cookie */

uint getASIOBaseOutputChannel(PVOID param_1)

{
  uint uVar1;
  undefined1 auStackY_e8 [32];
  DWORD local_a8 [4];
  CHAR local_98 [128];
  ulonglong local_18;
  
                    /* 0x29a0  5  getASIOBaseOutputChannel */
  local_18 = DAT_180006000 ^ (ulonglong)auStackY_e8;
  local_a8[0] = 4;
  uVar1 = RegGetValueA((HKEY)0xffffffff80000001,"SOFTWARE\\OpenHPSDR\\Thetis-x64",
                       "ASIObaseoutchannel",0x10010,(LPDWORD)0x0,param_1,local_a8);
  if (uVar1 == 2) {
    uVar1 = RegGetValueA((HKEY)0xffffffff80000002,"SOFTWARE\\OpenHPSDR\\Thetis-x64",
                         "ASIObaseoutchannel",0x10010,(LPDWORD)0x0,param_1,local_a8);
  }
  FUN_180001630(local_98,0x80,"RegGetValue(dword) status = %d",(ulonglong)uVar1);
  OutputDebugStringA(local_98);
  return uVar1;
}



/* ========================================================================
   ENTRY: 180002a90
   NAME : getASIOInputMode
   SIG  : uint __fastcall getASIOInputMode(PVOID param_1)
   ======================================================================== */

/* WARNING: Function: __security_check_cookie replaced with injection: security_check_cookie */

uint getASIOInputMode(PVOID param_1)

{
  uint uVar1;
  undefined1 auStackY_e8 [32];
  DWORD local_a8 [4];
  CHAR local_98 [128];
  ulonglong local_18;
  
                    /* 0x2a90  8  getASIOInputMode */
  local_18 = DAT_180006000 ^ (ulonglong)auStackY_e8;
  local_a8[0] = 4;
  uVar1 = RegGetValueA((HKEY)0xffffffff80000001,"SOFTWARE\\OpenHPSDR\\Thetis-x64","ASIOinputmode",
                       0x10010,(LPDWORD)0x0,param_1,local_a8);
  if (uVar1 == 2) {
    uVar1 = RegGetValueA((HKEY)0xffffffff80000002,"SOFTWARE\\OpenHPSDR\\Thetis-x64","ASIOinputmode",
                         0x10010,(LPDWORD)0x0,param_1,local_a8);
  }
  FUN_180001630(local_98,0x80,"RegGetValue(dword) status = %d",(ulonglong)uVar1);
  OutputDebugStringA(local_98);
  return uVar1;
}



/* ========================================================================
   ENTRY: 180002b80
   NAME : GetCMasioVersion
   SIG  : undefined8 __fastcall GetCMasioVersion(void)
   ======================================================================== */

undefined8 GetCMasioVersion(void)

{
                    /* 0x2b80  1  GetCMasioVersion */
  return 0x3fc;
}



/* ========================================================================
   ENTRY: 180002ba0
   NAME : __security_check_cookie
   SIG  : void __cdecl __security_check_cookie(uintptr_t _StackCookie)
   ======================================================================== */

/* WARNING: This is an inlined function */

void __cdecl __security_check_cookie(uintptr_t _StackCookie)

{
  if ((_StackCookie == DAT_180006000) && ((short)(_StackCookie >> 0x30) == 0)) {
    return;
  }
  FUN_180002bc0();
  return;
}



/* ========================================================================
   ENTRY: 180002bc0
   NAME : FUN_180002bc0
   SIG  : undefined __fastcall FUN_180002bc0(void)
   ======================================================================== */

void FUN_180002bc0(void)

{
  code *pcVar1;
  
  pcVar1 = (code *)swi(0x29);
  (*pcVar1)(2);
  return;
}



/* ========================================================================
   ENTRY: 180002bd0
   NAME : FUN_180002bd0
   SIG  : undefined __fastcall FUN_180002bd0(size_t param_1)
   ======================================================================== */

void FUN_180002bd0(size_t param_1)

{
  int iVar1;
  void *pvVar2;
  
  pvVar2 = malloc(param_1);
  while( true ) {
    if (pvVar2 != (void *)0x0) {
      return;
    }
    iVar1 = _callnewh(param_1);
    if (iVar1 == 0) break;
    pvVar2 = malloc(param_1);
  }
  if (param_1 == 0xffffffffffffffff) {
                    /* WARNING: Subroutine does not return */
    FUN_180003180();
  }
                    /* WARNING: Subroutine does not return */
  FUN_180003160();
}



/* ========================================================================
   ENTRY: 180002c20
   NAME : free
   SIG  : void __cdecl free(void * _Memory)
   ======================================================================== */

void __cdecl free(void *_Memory)

{
                    /* WARNING: Could not recover jumptable at 0x000180003a16. Too many branches */
                    /* WARNING: Treating indirect jump as call */
  free(_Memory);
  return;
}



/* ========================================================================
   ENTRY: 180002c30
   NAME : thunk_FUN_180002bd0
   SIG  : undefined __fastcall thunk_FUN_180002bd0(size_t param_1)
   ======================================================================== */

void thunk_FUN_180002bd0(size_t param_1)

{
  int iVar1;
  void *pvVar2;
  
  pvVar2 = malloc(param_1);
  while( true ) {
    if (pvVar2 != (void *)0x0) {
      return;
    }
    iVar1 = _callnewh(param_1);
    if (iVar1 == 0) break;
    pvVar2 = malloc(param_1);
  }
  if (param_1 == 0xffffffffffffffff) {
                    /* WARNING: Subroutine does not return */
    FUN_180003180();
  }
                    /* WARNING: Subroutine does not return */
  FUN_180003160();
}



/* ========================================================================
   ENTRY: 180002c40
   NAME : FUN_180002c40
   SIG  : ulonglong __fastcall FUN_180002c40(undefined8 param_1, int param_2, longlong param_3)
   ======================================================================== */

ulonglong FUN_180002c40(undefined8 param_1,int param_2,longlong param_3)

{
  byte bVar1;
  ulonglong uVar2;
  
  if (param_2 == 0) {
    uVar2 = FUN_180002db0(param_3 != 0);
    return uVar2;
  }
  if (param_2 == 1) {
    uVar2 = FUN_180002ca0(param_1,param_3);
    return uVar2;
  }
  if (param_2 == 2) {
    bVar1 = FUN_1800033f0();
    return (ulonglong)bVar1;
  }
  if (param_2 != 3) {
    return 1;
  }
  bVar1 = FUN_180003420();
  return (ulonglong)bVar1;
}



/* ========================================================================
   ENTRY: 180002ca0
   NAME : FUN_180002ca0
   SIG  : undefined8 __fastcall FUN_180002ca0(undefined8 param_1, undefined8 param_2)
   ======================================================================== */

undefined8 FUN_180002ca0(undefined8 param_1,undefined8 param_2)

{
  code *pcVar1;
  bool bVar2;
  undefined4 uVar3;
  int iVar4;
  undefined8 uVar5;
  undefined8 uVar6;
  longlong *plVar7;
  undefined *puVar8;
  
  uVar5 = FUN_1800034f0(0);
  if ((char)uVar5 != '\0') {
    uVar5 = FUN_1800032e0();
    bVar2 = true;
    if (DAT_180006148 != 0) {
      FUN_180003640(7);
      pcVar1 = (code *)swi(3);
      uVar5 = (*pcVar1)();
      return uVar5;
    }
    DAT_180006148 = 1;
    uVar3 = FUN_180003370();
    if ((char)uVar3 != '\0') {
      FUN_180003650();
      FUN_180003290();
      FUN_1800032c0();
      iVar4 = _initterm_e(&DAT_1800041d0,&DAT_1800041d8);
      if (iVar4 == 0) {
        uVar6 = FUN_180003330();
        if ((char)uVar6 != '\0') {
          _initterm(&DAT_1800041c0,&DAT_1800041c8);
          DAT_180006148 = 2;
          bVar2 = false;
        }
      }
    }
    FUN_1800035d0((char)uVar5);
    if (!bVar2) {
      plVar7 = (longlong *)FUN_180003630();
      if (*plVar7 != 0) {
        puVar8 = FUN_180003530((longlong)plVar7);
        if ((char)puVar8 != '\0') {
          (*(code *)PTR__guard_dispatch_icall_180004198)(param_1,2,param_2);
        }
      }
      DAT_180006120 = DAT_180006120 + 1;
      return 1;
    }
  }
  return 0;
}



/* ========================================================================
   ENTRY: 180002db0
   NAME : FUN_180002db0
   SIG  : ulonglong __fastcall FUN_180002db0(byte param_1)
   ======================================================================== */

ulonglong FUN_180002db0(byte param_1)

{
  code *pcVar1;
  byte bVar2;
  undefined8 uVar3;
  ulonglong uVar4;
  
  if (DAT_180006120 < 1) {
    return 0;
  }
  DAT_180006120 = DAT_180006120 + -1;
  uVar3 = FUN_1800032e0();
  if (DAT_180006148 == 2) {
    FUN_180003490();
    FUN_1800032a0();
    FUN_1800036a0();
    DAT_180006148 = 0;
    FUN_1800035d0((char)uVar3);
    bVar2 = FUN_180003600((ulonglong)param_1,'\0');
    FUN_1800034d0();
    return (ulonglong)bVar2;
  }
  FUN_180003640(7);
  pcVar1 = (code *)swi(3);
  uVar4 = (*pcVar1)();
  return uVar4;
}



/* ========================================================================
   ENTRY: 180002e50
   NAME : FUN_180002e50
   SIG  : ulonglong __fastcall FUN_180002e50(undefined8 param_1, int param_2, longlong param_3)
   ======================================================================== */

ulonglong FUN_180002e50(undefined8 param_1,int param_2,longlong param_3)

{
  uint uVar1;
  ulonglong uVar2;
  ulonglong uVar3;
  
  if ((param_2 == 0) && (DAT_180006120 < 1)) {
    return 0;
  }
  if (param_2 - 1U < 2) {
    if (DAT_180004200 == 0) {
      uVar2 = 1;
    }
    else {
      uVar1 = (*(code *)PTR__guard_dispatch_icall_180004198)();
      uVar2 = (ulonglong)uVar1;
    }
    if ((int)uVar2 == 0) {
      return uVar2;
    }
    uVar2 = FUN_180002c40(param_1,param_2,param_3);
    if ((int)uVar2 == 0) {
      return uVar2 & 0xffffffff;
    }
  }
  uVar2 = FUN_180001610();
  uVar3 = uVar2 & 0xffffffff;
  if ((param_2 == 1) && ((int)uVar2 == 0)) {
    FUN_180001610();
    FUN_180002db0(param_3 != 0);
    if (DAT_180004200 != 0) {
      (*(code *)PTR__guard_dispatch_icall_180004198)(param_1,0,param_3);
    }
  }
  if ((param_2 == 0) || (param_2 == 3)) {
    uVar2 = FUN_180002c40(param_1,param_2,param_3);
    uVar3 = uVar2 & 0xffffffff;
    if ((int)uVar2 != 0) {
      if (DAT_180004200 == 0) {
        uVar3 = 1;
      }
      else {
        uVar1 = (*(code *)PTR__guard_dispatch_icall_180004198)(param_1,param_2,param_3);
        uVar3 = (ulonglong)uVar1;
      }
    }
  }
  return uVar3;
}



/* ========================================================================
   ENTRY: 180002f90
   NAME : entry
   SIG  : undefined __fastcall entry(undefined8 param_1, int param_2, longlong param_3)
   ======================================================================== */

void entry(undefined8 param_1,int param_2,longlong param_3)

{
  if (param_2 == 1) {
    FUN_1800031d0();
  }
  FUN_180002e50(param_1,param_2,param_3);
  return;
}



/* ========================================================================
   ENTRY: 180002fd0
   NAME : FUN_180002fd0
   SIG  : undefined8 * __fastcall FUN_180002fd0(undefined8 * param_1, longlong param_2)
   ======================================================================== */

undefined8 * FUN_180002fd0(undefined8 *param_1,longlong param_2)

{
  *param_1 = std::exception::vftable;
  param_1[1] = 0;
  param_1[2] = 0;
  __std_exception_copy(param_2 + 8);
  *param_1 = std::bad_alloc::vftable;
  return param_1;
}



/* ========================================================================
   ENTRY: 180003010
   NAME : FUN_180003010
   SIG  : undefined8 * __fastcall FUN_180003010(undefined8 * param_1)
   ======================================================================== */

undefined8 * FUN_180003010(undefined8 *param_1)

{
  param_1[2] = 0;
  param_1[1] = "bad allocation";
  *param_1 = std::bad_alloc::vftable;
  return param_1;
}



/* ========================================================================
   ENTRY: 180003040
   NAME : FUN_180003040
   SIG  : undefined8 * __fastcall FUN_180003040(undefined8 * param_1, longlong param_2)
   ======================================================================== */

undefined8 * FUN_180003040(undefined8 *param_1,longlong param_2)

{
  *param_1 = std::exception::vftable;
  param_1[1] = 0;
  param_1[2] = 0;
  __std_exception_copy(param_2 + 8);
  *param_1 = std::bad_array_new_length::vftable;
  return param_1;
}



/* ========================================================================
   ENTRY: 180003080
   NAME : FUN_180003080
   SIG  : undefined8 * __fastcall FUN_180003080(undefined8 * param_1)
   ======================================================================== */

undefined8 * FUN_180003080(undefined8 *param_1)

{
  param_1[2] = 0;
  param_1[1] = "bad array new length";
  *param_1 = std::bad_array_new_length::vftable;
  return param_1;
}



/* ========================================================================
   ENTRY: 1800030b0
   NAME : exception
   SIG  : undefined __thiscall exception(exception * this, exception * param_1)
   ======================================================================== */

/* Library Function - Single Match
    public: __cdecl std::exception::exception(class std::exception const & __ptr64) __ptr64
   
   Library: Visual Studio 2019 Release */

exception * __thiscall std::exception::exception(exception *this,exception *param_1)

{
  *(undefined ***)this = vftable;
  *(undefined8 *)(this + 8) = 0;
  *(undefined8 *)(this + 0x10) = 0;
  __std_exception_copy(param_1 + 8);
  return this;
}



/* ========================================================================
   ENTRY: 180003110
   NAME : FUN_180003110
   SIG  : undefined8 * __fastcall FUN_180003110(undefined8 * param_1, ulonglong param_2)
   ======================================================================== */

undefined8 * FUN_180003110(undefined8 *param_1,ulonglong param_2)

{
  *param_1 = std::exception::vftable;
  __std_exception_destroy(param_1 + 1);
  if ((param_2 & 1) != 0) {
    free(param_1);
  }
  return param_1;
}



/* ========================================================================
   ENTRY: 180003160
   NAME : FUN_180003160
   SIG  : noreturn undefined __fastcall FUN_180003160(void)
   ======================================================================== */

void FUN_180003160(void)

{
  undefined8 local_28 [5];
  
  FUN_180003010(local_28);
                    /* WARNING: Subroutine does not return */
  _CxxThrowException(local_28,(ThrowInfo *)&DAT_180005260);
}



/* ========================================================================
   ENTRY: 180003180
   NAME : FUN_180003180
   SIG  : noreturn undefined __fastcall FUN_180003180(void)
   ======================================================================== */

void FUN_180003180(void)

{
  undefined8 local_28 [5];
  
  FUN_180003080(local_28);
                    /* WARNING: Subroutine does not return */
  _CxxThrowException(local_28,(ThrowInfo *)&DAT_1800052e8);
}



/* ========================================================================
   ENTRY: 1800031a0
   NAME : FUN_1800031a0
   SIG  : char * __fastcall FUN_1800031a0(longlong param_1)
   ======================================================================== */

char * FUN_1800031a0(longlong param_1)

{
  char *pcVar1;
  
  pcVar1 = "Unknown exception";
  if (*(char **)(param_1 + 8) != (char *)0x0) {
    pcVar1 = *(char **)(param_1 + 8);
  }
  return pcVar1;
}



/* ========================================================================
   ENTRY: 1800031c0
   NAME : free
   SIG  : void __cdecl free(void * _Memory)
   ======================================================================== */

void __cdecl free(void *_Memory)

{
                    /* WARNING: Could not recover jumptable at 0x000180003a16. Too many branches */
                    /* WARNING: Treating indirect jump as call */
  free(_Memory);
  return;
}



/* ========================================================================
   ENTRY: 1800031d0
   NAME : FUN_1800031d0
   SIG  : undefined __fastcall FUN_1800031d0(void)
   ======================================================================== */

/* WARNING: Globals starting with '_' overlap smaller symbols at the same address */

void FUN_1800031d0(void)

{
  DWORD DVar1;
  _FILETIME local_res8;
  LARGE_INTEGER local_res10 [3];
  _FILETIME local_18 [2];
  
  if (DAT_180006000 != 0x2b992ddfa232) {
    _DAT_180006040 = ~DAT_180006000;
    return;
  }
  local_res8.dwLowDateTime = 0;
  local_res8.dwHighDateTime = 0;
  GetSystemTimeAsFileTime(&local_res8);
  local_18[0] = local_res8;
  DVar1 = GetCurrentThreadId();
  local_18[0] = (_FILETIME)((ulonglong)local_18[0] ^ (ulonglong)DVar1);
  DVar1 = GetCurrentProcessId();
  local_18[0] = (_FILETIME)((ulonglong)local_18[0] ^ (ulonglong)DVar1);
  QueryPerformanceCounter(local_res10);
  DAT_180006000 =
       (local_res10[0].QuadPart << 0x20 ^ local_res10[0].QuadPart ^ (ulonglong)local_18[0] ^
       (ulonglong)local_18) & 0xffffffffffff;
  if (DAT_180006000 == 0x2b992ddfa232) {
    DAT_180006000 = 0x2b992ddfa233;
  }
  _DAT_180006040 = ~DAT_180006000;
  return;
}



/* ========================================================================
   ENTRY: 180003290
   NAME : FUN_180003290
   SIG  : undefined __fastcall FUN_180003290(void)
   ======================================================================== */

void FUN_180003290(void)

{
                    /* WARNING: Could not recover jumptable at 0x000180003297. Too many branches */
                    /* WARNING: Treating indirect jump as call */
  InitializeSListHead(&DAT_180006130);
  return;
}



/* ========================================================================
   ENTRY: 1800032a0
   NAME : FUN_1800032a0
   SIG  : undefined __fastcall FUN_1800032a0(void)
   ======================================================================== */

void FUN_1800032a0(void)

{
  __std_type_info_destroy_list(&DAT_180006130);
  return;
}



/* ========================================================================
   ENTRY: 1800032b0
   NAME : FUN_1800032b0
   SIG  : undefined * __fastcall FUN_1800032b0(void)
   ======================================================================== */

undefined * FUN_1800032b0(void)

{
  return &DAT_180006140;
}



/* ========================================================================
   ENTRY: 1800032c0
   NAME : FUN_1800032c0
   SIG  : undefined __fastcall FUN_1800032c0(void)
   ======================================================================== */

void FUN_1800032c0(void)

{
  ulonglong *puVar1;
  
  puVar1 = (ulonglong *)FUN_180001620();
  *puVar1 = *puVar1 | 0x24;
  puVar1 = (ulonglong *)FUN_1800032b0();
  *puVar1 = *puVar1 | 2;
  return;
}



/* ========================================================================
   ENTRY: 1800032e0
   NAME : FUN_1800032e0
   SIG  : undefined8 __fastcall FUN_1800032e0(void)
   ======================================================================== */

ulonglong FUN_1800032e0(void)

{
  ulonglong uVar1;
  ulonglong uVar2;
  bool bVar3;
  undefined7 extraout_var;
  ulonglong uVar4;
  
  bVar3 = FUN_1800039d0();
  uVar4 = CONCAT71(extraout_var,bVar3);
  if ((int)uVar4 != 0) {
    uVar1 = *(ulonglong *)((longlong)Self + 8);
    uVar4 = 0;
    LOCK();
    bVar3 = DAT_180006150 == 0;
    uVar2 = uVar1;
    if (!bVar3) {
      uVar4 = DAT_180006150;
      uVar2 = DAT_180006150;
    }
    DAT_180006150 = uVar2;
    UNLOCK();
    uVar2 = DAT_180006150;
    while (DAT_180006150 = uVar2, !bVar3) {
      if (uVar1 == uVar4) {
        return CONCAT71((int7)(uVar4 >> 8),1);
      }
      uVar4 = 0;
      LOCK();
      bVar3 = uVar2 == 0;
      DAT_180006150 = uVar1;
      if (!bVar3) {
        uVar4 = uVar2;
        DAT_180006150 = uVar2;
      }
      UNLOCK();
      uVar2 = DAT_180006150;
    }
  }
  return uVar4 & 0xffffffffffffff00;
}



/* ========================================================================
   ENTRY: 180003330
   NAME : FUN_180003330
   SIG  : undefined8 __fastcall FUN_180003330(void)
   ======================================================================== */

undefined8 FUN_180003330(void)

{
  bool bVar1;
  undefined7 extraout_var;
  undefined8 uVar2;
  ulonglong uVar3;
  
  bVar1 = FUN_1800039d0();
  if ((int)CONCAT71(extraout_var,bVar1) != 0) {
    uVar2 = FUN_180003720();
    return CONCAT71((int7)((ulonglong)uVar2 >> 8),1);
  }
  uVar3 = FUN_180001610();
  uVar3 = _configure_narrow_argv(uVar3 & 0xffffffff);
  if ((int)uVar3 != 0) {
    return uVar3 & 0xffffffffffffff00;
  }
  uVar2 = _initialize_narrow_environment();
  return CONCAT71((int7)((ulonglong)uVar2 >> 8),1);
}



/* ========================================================================
   ENTRY: 180003370
   NAME : FUN_180003370
   SIG  : undefined4 __fastcall FUN_180003370(void)
   ======================================================================== */

/* WARNING: Globals starting with '_' overlap smaller symbols at the same address */

uint FUN_180003370(void)

{
  bool bVar1;
  undefined4 in_EAX;
  undefined3 extraout_var;
  uint uVar2;
  
  if (DAT_180006159 == '\0') {
    bVar1 = FUN_1800039d0();
    if (CONCAT31(extraout_var,bVar1) == 0) {
      _DAT_180006160 = _DAT_180004290;
      uRam0000000180006168 = _UNK_180004298;
      _DAT_180006170 = 0xffffffffffffffff;
      _DAT_180006178 = _DAT_180004290;
      uRam0000000180006180 = _UNK_180004298;
      _DAT_180006188 = 0xffffffffffffffff;
    }
    else {
      uVar2 = _initialize_onexit_table(&DAT_180006160);
      if (uVar2 != 0) {
LAB_1800033a6:
        return uVar2 & 0xffffff00;
      }
      uVar2 = _initialize_onexit_table(&DAT_180006178);
      if (uVar2 != 0) goto LAB_1800033a6;
    }
    in_EAX = 0;
    DAT_180006159 = '\x01';
  }
  return CONCAT31((int3)((uint)in_EAX >> 8),1);
}



/* ========================================================================
   ENTRY: 1800033f0
   NAME : FUN_1800033f0
   SIG  : undefined1 __fastcall FUN_1800033f0(void)
   ======================================================================== */

undefined1 FUN_1800033f0(void)

{
  char cVar1;
  
  cVar1 = FUN_180003a40();
  if (cVar1 != '\0') {
    cVar1 = FUN_180003a40();
    if (cVar1 != '\0') {
      return 1;
    }
    FUN_180003a40();
  }
  return 0;
}



/* ========================================================================
   ENTRY: 180003420
   NAME : FUN_180003420
   SIG  : undefined1 __fastcall FUN_180003420(void)
   ======================================================================== */

undefined1 FUN_180003420(void)

{
  FUN_180003a40();
  FUN_180003a40();
  return 1;
}



/* ========================================================================
   ENTRY: 180003440
   NAME : FUN_180003440
   SIG  : undefined __fastcall FUN_180003440(undefined8 param_1, int param_2, undefined8 param_3, undefined * param_4, undefined4 param_5, undefined8 param_6)
   ======================================================================== */

void FUN_180003440(undefined8 param_1,int param_2,undefined8 param_3,undefined *param_4,
                  undefined4 param_5,undefined8 param_6)

{
  bool bVar1;
  undefined7 extraout_var;
  
  bVar1 = FUN_1800039d0();
  if (((int)CONCAT71(extraout_var,bVar1) == 0) && (param_2 == 1)) {
    (*(code *)PTR__guard_dispatch_icall_180004198)(param_1,0,param_3);
  }
  _seh_filter_dll(param_5,param_6);
  return;
}



/* ========================================================================
   ENTRY: 180003490
   NAME : FUN_180003490
   SIG  : undefined __fastcall FUN_180003490(void)
   ======================================================================== */

void FUN_180003490(void)

{
  bool bVar1;
  undefined7 extraout_var;
  undefined8 uVar2;
  
  bVar1 = FUN_1800039d0();
  if ((int)CONCAT71(extraout_var,bVar1) != 0) {
    _execute_onexit_table(&DAT_180006160);
    return;
  }
  uVar2 = FUN_180003a50();
  if ((int)uVar2 == 0) {
    _cexit();
    return;
  }
  return;
}



/* ========================================================================
   ENTRY: 1800034d0
   NAME : FUN_1800034d0
   SIG  : undefined __fastcall FUN_1800034d0(void)
   ======================================================================== */

void FUN_1800034d0(void)

{
  FUN_180003a40();
  FUN_180003a40();
  return;
}



/* ========================================================================
   ENTRY: 1800034f0
   NAME : FUN_1800034f0
   SIG  : undefined8 __fastcall FUN_1800034f0(int param_1)
   ======================================================================== */

longlong FUN_1800034f0(int param_1)

{
  char cVar1;
  uint7 extraout_var;
  undefined7 extraout_var_00;
  uint7 extraout_var_01;
  uint7 uVar2;
  
  if (param_1 == 0) {
    DAT_180006158 = 1;
  }
  FUN_180003720();
  cVar1 = FUN_180003a40();
  uVar2 = extraout_var;
  if (cVar1 != '\0') {
    cVar1 = FUN_180003a40();
    if (cVar1 != '\0') {
      return CONCAT71(extraout_var_00,1);
    }
    FUN_180003a40();
    uVar2 = extraout_var_01;
  }
  return (ulonglong)uVar2 << 8;
}



/* ========================================================================
   ENTRY: 180003530
   NAME : FUN_180003530
   SIG  : undefined * __fastcall FUN_180003530(longlong param_1)
   ======================================================================== */

undefined * FUN_180003530(longlong param_1)

{
  ulonglong uVar1;
  ulonglong uVar2;
  uint7 uVar3;
  longlong lVar4;
  
  uVar1 = 0x5a4d;
  if (IMAGE_DOS_HEADER_180000000.e_magic == (char  [2])0x5a4d) {
    lVar4 = (longlong)(int)IMAGE_DOS_HEADER_180000000.e_lfanew;
    if ((*(int *)(lVar4 + 0x180000000) == 0x4550) &&
       (uVar1 = 0x20b,
       *(short *)((longlong)IMAGE_DOS_HEADER_180000000.e_res_4_ + lVar4 + -4) == 0x20b)) {
      uVar2 = (ulonglong)*(ushort *)((longlong)IMAGE_DOS_HEADER_180000000.e_res_4_ + lVar4 + -8) +
              0x18 + lVar4 + 0x180000000;
      uVar1 = uVar2 + (ulonglong)*(ushort *)(IMAGE_DOS_HEADER_180000000.e_magic + lVar4 + 6) * 0x28;
      while( true ) {
        uVar3 = (uint7)(uVar2 >> 8);
        if (uVar2 == uVar1) {
          return (undefined *)((ulonglong)uVar3 << 8);
        }
        if (((ulonglong)*(uint *)(uVar2 + 0xc) <= param_1 - 0x180000000U) &&
           (param_1 - 0x180000000U < (ulonglong)(*(int *)(uVar2 + 8) + *(uint *)(uVar2 + 0xc))))
        break;
        uVar2 = uVar2 + 0x28;
      }
      if (*(int *)(uVar2 + 0x24) < 0) {
        return (undefined *)(uVar2 & 0xffffffffffffff00);
      }
      return (undefined *)CONCAT71(uVar3,1);
    }
  }
  return (undefined *)(uVar1 & 0xffffffffffffff00);
}



/* ========================================================================
   ENTRY: 1800035d0
   NAME : FUN_1800035d0
   SIG  : undefined8 __fastcall FUN_1800035d0(char param_1)
   ======================================================================== */

undefined8 FUN_1800035d0(char param_1)

{
  undefined8 uVar1;
  bool bVar2;
  undefined7 extraout_var;
  undefined8 uVar3;
  
  bVar2 = FUN_1800039d0();
  uVar1 = DAT_180006150;
  uVar3 = CONCAT71(extraout_var,bVar2);
  if (((int)uVar3 != 0) && (param_1 == '\0')) {
    LOCK();
    DAT_180006150 = 0;
    UNLOCK();
    uVar3 = uVar1;
  }
  return uVar3;
}



/* ========================================================================
   ENTRY: 180003600
   NAME : FUN_180003600
   SIG  : undefined1 __fastcall FUN_180003600(undefined8 param_1, char param_2)
   ======================================================================== */

undefined1 FUN_180003600(undefined8 param_1,char param_2)

{
  if ((DAT_180006158 == '\0') || (param_2 == '\0')) {
    FUN_180003a40();
    FUN_180003a40();
  }
  return 1;
}



/* ========================================================================
   ENTRY: 180003630
   NAME : FUN_180003630
   SIG  : undefined * __fastcall FUN_180003630(void)
   ======================================================================== */

undefined * FUN_180003630(void)

{
  return &DAT_1800064d8;
}



/* ========================================================================
   ENTRY: 180003640
   NAME : FUN_180003640
   SIG  : undefined __fastcall FUN_180003640(undefined4 param_1)
   ======================================================================== */

void FUN_180003640(undefined4 param_1)

{
  code *pcVar1;
  
  pcVar1 = (code *)swi(0x29);
  (*pcVar1)(param_1);
  return;
}



/* ========================================================================
   ENTRY: 180003650
   NAME : FUN_180003650
   SIG  : undefined __fastcall FUN_180003650(void)
   ======================================================================== */

/* WARNING: Removing unreachable block (ram,0x000180003670) */
/* WARNING: Removing unreachable block (ram,0x000180003678) */
/* WARNING: Removing unreachable block (ram,0x00018000367e) */

void FUN_180003650(void)

{
  return;
}



/* ========================================================================
   ENTRY: 1800036a0
   NAME : FUN_1800036a0
   SIG  : undefined __fastcall FUN_1800036a0(void)
   ======================================================================== */

/* WARNING: Removing unreachable block (ram,0x0001800036c0) */
/* WARNING: Removing unreachable block (ram,0x0001800036c8) */
/* WARNING: Removing unreachable block (ram,0x0001800036ce) */

void FUN_1800036a0(void)

{
  return;
}



/* ========================================================================
   ENTRY: 1800036f0
   NAME : FUN_1800036f0
   SIG  : undefined8 * __fastcall FUN_1800036f0(undefined8 * param_1, ulonglong param_2)
   ======================================================================== */

undefined8 * FUN_1800036f0(undefined8 *param_1,ulonglong param_2)

{
  *param_1 = type_info::vftable;
  if ((param_2 & 1) != 0) {
    free(param_1);
  }
  return param_1;
}



/* ========================================================================
   ENTRY: 180003720
   NAME : FUN_180003720
   SIG  : undefined8 __fastcall FUN_180003720(void)
   ======================================================================== */

/* WARNING: Removing unreachable block (ram,0x000180003827) */
/* WARNING: Removing unreachable block (ram,0x000180003815) */
/* WARNING: Removing unreachable block (ram,0x000180003803) */
/* WARNING: Removing unreachable block (ram,0x0001800037dc) */
/* WARNING: Removing unreachable block (ram,0x000180003757) */
/* WARNING: Removing unreachable block (ram,0x000180003732) */
/* WARNING: Globals starting with '_' overlap smaller symbols at the same address */

undefined8 FUN_180003720(void)

{
  int *piVar1;
  uint *puVar2;
  longlong lVar3;
  int iVar4;
  uint uVar5;
  ulonglong uVar6;
  byte bVar7;
  uint uVar8;
  ulonglong uVar9;
  uint uVar10;
  uint uVar11;
  uint uVar12;
  uint uVar13;
  ulonglong in_XCR0;
  
  piVar1 = (int *)cpuid_basic_info(0);
  iVar4 = *piVar1;
  puVar2 = (uint *)cpuid_Version_info(1);
  uVar5 = puVar2[3];
  if ((piVar1[2] == 0x49656e69 && piVar1[3] == 0x6c65746e) && piVar1[1] == 0x756e6547) {
    uVar8 = *puVar2 & 0xfff3ff0;
    _DAT_180006058 = 0x8000;
    _DAT_180006060 = 0xffffffffffffffff;
    if ((((uVar8 == 0x106c0) || (uVar8 == 0x20660)) || (uVar8 == 0x20670)) ||
       ((uVar8 - 0x30650 < 0x21 &&
        ((0x100010001U >> ((ulonglong)(uVar8 - 0x30650) & 0x3f) & 1) != 0)))) {
      DAT_180006194 = DAT_180006194 | 1;
    }
  }
  uVar13 = 0;
  uVar8 = 0;
  uVar11 = 0;
  if (iVar4 < 7) {
    uVar12 = 0;
    uVar10 = 0;
  }
  else {
    piVar1 = (int *)cpuid_Extended_Feature_Enumeration_info(7);
    uVar12 = piVar1[1];
    uVar10 = piVar1[2];
    if ((uVar12 >> 9 & 1) != 0) {
      DAT_180006194 = DAT_180006194 | 2;
    }
    if (0 < *piVar1) {
      lVar3 = cpuid_Extended_Feature_Enumeration_info(7);
      uVar13 = *(uint *)(lVar3 + 8);
    }
    if (0x23 < iVar4) {
      lVar3 = cpuid(0x24);
      uVar8 = *(uint *)(lVar3 + 4);
    }
    uVar11 = 0;
    if (0x28 < iVar4) {
      lVar3 = cpuid(0x29);
      uVar11 = *(uint *)(lVar3 + 4);
    }
  }
  _DAT_180006050 = 1;
  DAT_180006054 = 2;
  uVar6 = DAT_180006068 & 0xfffffffffffffffe;
  if ((uVar5 >> 0x14 & 1) != 0) {
    _DAT_180006050 = 2;
    DAT_180006054 = 6;
    uVar6 = DAT_180006068 & 0xffffffffffffffee;
  }
  DAT_180006068 = uVar6;
  if ((uVar5 >> 0x1b & 1) != 0) {
    uVar6 = xinuse(0);
    uVar9 = in_XCR0 & uVar6 & 0xffffffff;
    uVar6 = DAT_180006068;
    if (((uVar5 >> 0x1c & 1) != 0) && (bVar7 = (byte)uVar9, (bVar7 & 6) == 6)) {
      _DAT_180006050 = 3;
      uVar5 = DAT_180006054 | 8;
      if ((uVar12 & 0x20) != 0) {
        _DAT_180006050 = 5;
        uVar5 = DAT_180006054 | 0x28;
        uVar6 = DAT_180006068 & 0xfffffffffffffffd;
        if (((uVar12 & 0xd0030000) == 0xd0030000) && ((bVar7 & 0xe0) == 0xe0)) {
          DAT_180006054 = DAT_180006054 | 0x68;
          _DAT_180006050 = 6;
          uVar5 = DAT_180006054;
          uVar6 = DAT_180006068 & 0xffffffffffffffd9;
        }
      }
      DAT_180006068 = uVar6;
      DAT_180006054 = uVar5;
      if ((uVar10 >> 0x17 & 1) != 0) {
        DAT_180006068 = DAT_180006068 & 0xfffffffffeffffff;
      }
      uVar6 = DAT_180006068;
      if (((uVar13 >> 0x13 & 1) != 0) && ((bVar7 & 0xe0) == 0xe0)) {
        _DAT_180006198 = uVar8 & 0xff;
        uVar6 = DAT_180006068 & 0xfffffffffeffffd0;
        if (1 < _DAT_180006198) {
          uVar6 = DAT_180006068 & 0xfffffffffeffff90;
        }
      }
    }
    DAT_180006068 = uVar6;
    if ((((uVar13 >> 0x15 & 1) != 0) && ((uVar11 & 1) != 0)) && ((uVar9 >> 0x13 & 1) != 0)) {
      DAT_180006068 = DAT_180006068 & 0xffffffffffffff7f;
    }
  }
  return 0;
}



/* ========================================================================
   ENTRY: 1800039d0
   NAME : FUN_1800039d0
   SIG  : bool __fastcall FUN_1800039d0(void)
   ======================================================================== */

bool FUN_1800039d0(void)

{
  return DAT_180006070 != 0;
}



/* ========================================================================
   ENTRY: 1800039e6
   NAME : __std_exception_copy
   SIG  : undefined __std_exception_copy(void)
   ======================================================================== */

void __std_exception_copy(void)

{
                    /* WARNING: Could not recover jumptable at 0x0001800039e6. Too many branches */
                    /* WARNING: Treating indirect jump as call */
  __std_exception_copy();
  return;
}



/* ========================================================================
   ENTRY: 1800039ec
   NAME : __std_exception_destroy
   SIG  : undefined __std_exception_destroy(void)
   ======================================================================== */

void __std_exception_destroy(void)

{
                    /* WARNING: Could not recover jumptable at 0x0001800039ec. Too many branches */
                    /* WARNING: Treating indirect jump as call */
  __std_exception_destroy();
  return;
}



/* ========================================================================
   ENTRY: 1800039f2
   NAME : _CxxThrowException
   SIG  : noreturn void __stdcall _CxxThrowException(void * pExceptionObject, ThrowInfo * pThrowInfo)
   ======================================================================== */

void __stdcall _CxxThrowException(void *pExceptionObject,ThrowInfo *pThrowInfo)

{
                    /* WARNING: Could not recover jumptable at 0x0001800039f2. Too many branches */
                    /* WARNING: Subroutine does not return */
                    /* WARNING: Treating indirect jump as call */
  _CxxThrowException(pExceptionObject,pThrowInfo);
  return;
}



/* ========================================================================
   ENTRY: 1800039f8
   NAME : __std_type_info_destroy_list
   SIG  : undefined __std_type_info_destroy_list(void)
   ======================================================================== */

void __std_type_info_destroy_list(void)

{
                    /* WARNING: Could not recover jumptable at 0x0001800039f8. Too many branches */
                    /* WARNING: Treating indirect jump as call */
  __std_type_info_destroy_list();
  return;
}



/* ========================================================================
   ENTRY: 1800039fe
   NAME : _callnewh
   SIG  : int __cdecl _callnewh(size_t _Size)
   ======================================================================== */

int __cdecl _callnewh(size_t _Size)

{
  int iVar1;
  
                    /* WARNING: Could not recover jumptable at 0x0001800039fe. Too many branches */
                    /* WARNING: Treating indirect jump as call */
  iVar1 = _callnewh(_Size);
  return iVar1;
}



/* ========================================================================
   ENTRY: 180003a04
   NAME : malloc
   SIG  : void * __cdecl malloc(size_t _Size)
   ======================================================================== */

void * __cdecl malloc(size_t _Size)

{
  void *pvVar1;
  
                    /* WARNING: Could not recover jumptable at 0x000180003a04. Too many branches */
                    /* WARNING: Treating indirect jump as call */
  pvVar1 = malloc(_Size);
  return pvVar1;
}



/* ========================================================================
   ENTRY: 180003a0a
   NAME : _initterm
   SIG  : undefined _initterm(void)
   ======================================================================== */

void _initterm(void)

{
                    /* WARNING: Could not recover jumptable at 0x000180003a0a. Too many branches */
                    /* WARNING: Treating indirect jump as call */
  _initterm();
  return;
}



/* ========================================================================
   ENTRY: 180003a10
   NAME : _initterm_e
   SIG  : undefined _initterm_e(void)
   ======================================================================== */

void _initterm_e(void)

{
                    /* WARNING: Could not recover jumptable at 0x000180003a10. Too many branches */
                    /* WARNING: Treating indirect jump as call */
  _initterm_e();
  return;
}



/* ========================================================================
   ENTRY: 180003a16
   NAME : free
   SIG  : void __cdecl free(void * _Memory)
   ======================================================================== */

void __cdecl free(void *_Memory)

{
                    /* WARNING: Could not recover jumptable at 0x000180003a16. Too many branches */
                    /* WARNING: Treating indirect jump as call */
  free(_Memory);
  return;
}



/* ========================================================================
   ENTRY: 180003a1c
   NAME : _seh_filter_dll
   SIG  : undefined _seh_filter_dll(void)
   ======================================================================== */

void _seh_filter_dll(void)

{
                    /* WARNING: Could not recover jumptable at 0x000180003a1c. Too many branches */
                    /* WARNING: Treating indirect jump as call */
  _seh_filter_dll();
  return;
}



/* ========================================================================
   ENTRY: 180003a22
   NAME : _configure_narrow_argv
   SIG  : undefined _configure_narrow_argv(void)
   ======================================================================== */

void _configure_narrow_argv(void)

{
                    /* WARNING: Could not recover jumptable at 0x000180003a22. Too many branches */
                    /* WARNING: Treating indirect jump as call */
  _configure_narrow_argv();
  return;
}



/* ========================================================================
   ENTRY: 180003a28
   NAME : _initialize_narrow_environment
   SIG  : undefined _initialize_narrow_environment(void)
   ======================================================================== */

void _initialize_narrow_environment(void)

{
                    /* WARNING: Could not recover jumptable at 0x000180003a28. Too many branches */
                    /* WARNING: Treating indirect jump as call */
  _initialize_narrow_environment();
  return;
}



/* ========================================================================
   ENTRY: 180003a2e
   NAME : _initialize_onexit_table
   SIG  : undefined _initialize_onexit_table(void)
   ======================================================================== */

void _initialize_onexit_table(void)

{
                    /* WARNING: Could not recover jumptable at 0x000180003a2e. Too many branches */
                    /* WARNING: Treating indirect jump as call */
  _initialize_onexit_table();
  return;
}



/* ========================================================================
   ENTRY: 180003a34
   NAME : _execute_onexit_table
   SIG  : undefined _execute_onexit_table(void)
   ======================================================================== */

void _execute_onexit_table(void)

{
                    /* WARNING: Could not recover jumptable at 0x000180003a34. Too many branches */
                    /* WARNING: Treating indirect jump as call */
  _execute_onexit_table();
  return;
}



/* ========================================================================
   ENTRY: 180003a3a
   NAME : _cexit
   SIG  : void __cdecl _cexit(void)
   ======================================================================== */

void __cdecl _cexit(void)

{
                    /* WARNING: Could not recover jumptable at 0x000180003a3a. Too many branches */
                    /* WARNING: Treating indirect jump as call */
  _cexit();
  return;
}



/* ========================================================================
   ENTRY: 180003a40
   NAME : FUN_180003a40
   SIG  : undefined1 __fastcall FUN_180003a40(void)
   ======================================================================== */

undefined1 FUN_180003a40(void)

{
  return 1;
}



/* ========================================================================
   ENTRY: 180003a50
   NAME : FUN_180003a50
   SIG  : undefined8 __fastcall FUN_180003a50(void)
   ======================================================================== */

undefined8 FUN_180003a50(void)

{
  return 0;
}



/* ========================================================================
   ENTRY: 180003a60
   NAME : FUN_180003a60
   SIG  : undefined8 __fastcall FUN_180003a60(undefined8 param_1, undefined8 param_2, undefined8 param_3, longlong param_4)
   ======================================================================== */

undefined8 FUN_180003a60(undefined8 param_1,undefined8 param_2,undefined8 param_3,longlong param_4)

{
  FUN_180003a80(param_2,param_4);
  return 1;
}



/* ========================================================================
   ENTRY: 180003a80
   NAME : FUN_180003a80
   SIG  : undefined __fastcall FUN_180003a80(undefined8 param_1, longlong param_2)
   ======================================================================== */

/* WARNING: Function: __security_check_cookie replaced with injection: security_check_cookie */

void FUN_180003a80(undefined8 param_1,longlong param_2)

{
  code *pcVar1;
  byte bVar2;
  uint uVar3;
  byte bVar4;
  byte *pbVar5;
  byte *pbVar6;
  
  pbVar5 = (byte *)((ulonglong)*(uint *)(*(longlong *)(param_2 + 0x10) + 8) +
                   *(longlong *)(param_2 + 8));
  bVar2 = pbVar5[3];
  if ((*pbVar5 & 7) < 3) {
    return;
  }
  if ((bVar2 & 0x1f) == 0) {
    return;
  }
  uVar3 = 0;
  pbVar5 = pbVar5 + (ulonglong)(bVar2 & 0x1f) + 4;
  pbVar6 = pbVar5;
  if (bVar2 >> 5 != 0) {
    do {
      pbVar5 = pbVar6 + 3;
      if (*pbVar6 >> 3 != 0) {
        pbVar5 = pbVar6 + (ulonglong)(uint)(*pbVar6 >> 3) + 6;
      }
      uVar3 = uVar3 + 1;
      pbVar6 = pbVar5;
    } while (uVar3 < bVar2 >> 5);
  }
  if ((bVar2 & 0x1f) == 0) {
    return;
  }
  bVar2 = *pbVar5;
  bVar4 = bVar2;
  if (((bVar2 < 4) || (bVar4 = bVar2 & 0xf, (byte)(bVar4 - 8) < 3)) ||
     (bVar4 = bVar2 & 7, (byte)(bVar4 - 4) < 4)) {
    if (bVar4 == 0) {
      return;
    }
    if (bVar2 < 4) goto LAB_180003b97;
    bVar4 = bVar2 & 0xf;
    if (((byte)((bVar2 & 0xf) - 8) < 3) || (bVar4 = bVar2 & 7, (byte)((bVar2 & 7) - 4) < 4)) {
      bVar2 = bVar4;
      if (bVar2 < 0x21) goto LAB_180003b97;
      goto LAB_180003c09;
    }
  }
  if ((bVar2 & 0x3f) != 0x20) {
LAB_180003c09:
    FUN_180002bc0();
    pcVar1 = (code *)swi(3);
    (*pcVar1)();
    return;
  }
  bVar2 = 0x20;
LAB_180003b97:
                    /* WARNING: Could not emulate address calculation at 0x000180003ba2 */
                    /* WARNING: Treating indirect jump as call */
  (*(code *)((ulonglong)*(uint *)(&LAB_180003c10 + (ulonglong)(byte)(&DAT_180003c24)[bVar2] * 4) +
            0x180000000))
            ((code *)((ulonglong)
                      *(uint *)(&LAB_180003c10 + (ulonglong)(byte)(&DAT_180003c24)[bVar2] * 4) +
                     0x180000000));
  return;
}



/* ========================================================================
   ENTRY: 180003c45
   NAME : memset
   SIG  : void * __cdecl memset(void * _Dst, int _Val, size_t _Size)
   ======================================================================== */

void * __cdecl memset(void *_Dst,int _Val,size_t _Size)

{
  void *pvVar1;
  
                    /* WARNING: Could not recover jumptable at 0x000180003c45. Too many branches */
                    /* WARNING: Treating indirect jump as call */
  pvVar1 = memset(_Dst,_Val,_Size);
  return pvVar1;
}



/* ========================================================================
   ENTRY: 180003c4b
   NAME : strcmp
   SIG  : int __cdecl strcmp(char * _Str1, char * _Str2)
   ======================================================================== */

int __cdecl strcmp(char *_Str1,char *_Str2)

{
  int iVar1;
  
                    /* WARNING: Could not recover jumptable at 0x000180003c4b. Too many branches */
                    /* WARNING: Treating indirect jump as call */
  iVar1 = strcmp(_Str1,_Str2);
  return iVar1;
}



/* ========================================================================
   ENTRY: 180003c51
   NAME : strcpy
   SIG  : char * __cdecl strcpy(char * _Dest, char * _Source)
   ======================================================================== */

char * __cdecl strcpy(char *_Dest,char *_Source)

{
  char *pcVar1;
  
                    /* WARNING: Could not recover jumptable at 0x000180003c51. Too many branches */
                    /* WARNING: Treating indirect jump as call */
  pcVar1 = strcpy(_Dest,_Source);
  return pcVar1;
}



/* ========================================================================
   ENTRY: 180003c57
   NAME : strlen
   SIG  : size_t __cdecl strlen(char * _Str)
   ======================================================================== */

size_t __cdecl strlen(char *_Str)

{
  size_t sVar1;
  
                    /* WARNING: Could not recover jumptable at 0x000180003c57. Too many branches */
                    /* WARNING: Treating indirect jump as call */
  sVar1 = strlen(_Str);
  return sVar1;
}



/* ========================================================================
   ENTRY: 180003c80
   NAME : _guard_dispatch_icall
   SIG  : undefined __fastcall _guard_dispatch_icall(void)
   ======================================================================== */

/* WARNING: This is an inlined function */

void _guard_dispatch_icall(void)

{
  code *UNRECOVERED_JUMPTABLE;
  
                    /* WARNING: Could not recover jumptable at 0x000180003c80. Too many branches */
                    /* WARNING: Treating indirect jump as call */
  (*UNRECOVERED_JUMPTABLE)();
  return;
}



/* ========================================================================
   ENTRY: 180003ca0
   NAME : _guard_dispatch_icall
   SIG  : undefined __fastcall _guard_dispatch_icall(void)
   ======================================================================== */

/* WARNING: This is an inlined function */
/* WARNING: Switch with 1 destination removed at 0x000180003ca0 */

void _guard_dispatch_icall(void)

{
  code *UNRECOVERED_JUMPTABLE;
  
                    /* WARNING: Could not recover jumptable at 0x000180003c80. Too many branches */
                    /* WARNING: Treating indirect jump as call */
  (*UNRECOVERED_JUMPTABLE)();
  return;
}



/* ========================================================================
   ENTRY: 180003cb0
   NAME : FUN_180003cb0
   SIG  : undefined __fastcall FUN_180003cb0(undefined8 param_1, longlong param_2)
   ======================================================================== */

void FUN_180003cb0(undefined8 param_1,longlong param_2)

{
  FUN_1800035d0(*(char *)(param_2 + 0x60));
  return;
}



/* ========================================================================
   ENTRY: 180003cd0
   NAME : FUN_180003cd0
   SIG  : undefined __fastcall FUN_180003cd0(undefined8 param_1, longlong param_2)
   ======================================================================== */

void FUN_180003cd0(undefined8 param_1,longlong param_2)

{
  FUN_1800035d0(*(char *)(param_2 + 0x20));
  return;
}



/* ========================================================================
   ENTRY: 180003cea
   NAME : FUN_180003cea
   SIG  : undefined __fastcall FUN_180003cea(void)
   ======================================================================== */

void FUN_180003cea(void)

{
  FUN_1800034d0();
  return;
}



/* ========================================================================
   ENTRY: 180003d00
   NAME : FUN_180003d00
   SIG  : undefined __fastcall FUN_180003d00(undefined8 * param_1, longlong param_2)
   ======================================================================== */

void FUN_180003d00(undefined8 *param_1,longlong param_2)

{
  FUN_180003440(*(undefined8 *)(param_2 + 0x60),*(int *)(param_2 + 0x68),
                *(undefined8 *)(param_2 + 0x70),FUN_180002c40,*(undefined4 *)*param_1,param_1);
  return;
}



/* ========================================================================
   ENTRY: 180003d40
   NAME : FUN_180003d40
   SIG  : bool __fastcall FUN_180003d40(undefined8 * param_1)
   ======================================================================== */

bool FUN_180003d40(undefined8 *param_1)

{
  return *(int *)*param_1 == -0x3ffffffb;
}


