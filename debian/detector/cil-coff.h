
#ifndef __MONO_CIL_COFF_H__
#define __MONO_CIL_COFF_H__

/*
 * 25.2.1: Method header type values
 */
#define METHOD_HEADER_FORMAT_MASK   7
#define METHOD_HEADER_TINY_FORMAT   2
#define METHOD_HEADER_TINY_FORMAT1  6
#define METHOD_HEADER_FAT_FORMAT    3

/*
 * 25.2.3.1: Flags for method headers
 */
#define METHOD_HEADER_INIT_LOCALS   0x10
#define METHOD_HEADER_MORE_SECTS    0x08

/*
 * For section data (25.3)
 */
#define METHOD_HEADER_SECTION_RESERVED    0
#define METHOD_HEADER_SECTION_EHTABLE     1
#define METHOD_HEADER_SECTION_OPTIL_TABLE 2
#define METHOD_HEADER_SECTION_FAT_FORMAT  0x40
#define METHOD_HEADER_SECTION_MORE_SECTS  0x80

/* 128 bytes */
typedef struct {
	char    msdos_sig [2];
	unsigned short nlast_page;
	unsigned short npages;
	char    msdos_header [54];
	unsigned char pe_offset[4];
	char    msdos_header2 [64];
} MSDOSHeader;

/* 20 bytes */
typedef struct {
	unsigned short  coff_machine;
	unsigned short  coff_sections;
	unsigned long  coff_time;
	unsigned long  coff_symptr;
	unsigned long  coff_symcount;
	unsigned short  coff_opt_header_size;
	unsigned short  coff_attributes;
} COFFHeader;

#define COFF_ATTRIBUTE_EXECUTABLE_IMAGE 0x0002
#define COFF_ATTRIBUTE_LIBRARY_IMAGE    0x2000

/* 28 bytes */
typedef struct {
	unsigned char pe_magic[2];
	unsigned char  pe_major;
	unsigned char  pe_minor;
	unsigned long pe_code_size;
	unsigned long pe_data_size;
	unsigned long pe_uninit_data_size;
	unsigned long pe_rva_entry_point;
	unsigned long pe_rva_code_base;
	unsigned long pe_rva_data_base;
} PEHeader;

/* 68 bytes */
typedef struct {
	unsigned long pe_image_base;		/* must be 0x400000 */
	unsigned long pe_section_align;       /* must be 8192 */
	unsigned long pe_file_alignment;      /* must be 512 or 4096 */
	unsigned short pe_os_major;            /* must be 4 */
	unsigned short pe_os_minor;            /* must be 0 */
	unsigned short pe_user_major;
	unsigned short pe_user_minor;
	unsigned short pe_subsys_major;
	unsigned short pe_subsys_minor;
	unsigned long pe_reserved_1;
	unsigned long pe_image_size;
	unsigned long pe_header_size;
	unsigned long pe_checksum;
	unsigned short pe_subsys_required;
	unsigned short pe_dll_flags;
	unsigned long pe_stack_reserve;
	unsigned long pe_stack_commit;
	unsigned long pe_heap_reserve;
	unsigned long pe_heap_commit;
	unsigned long pe_loader_flags;
	unsigned long pe_data_dir_count;
} PEHeaderNT;

typedef struct {
	unsigned char rva[4];
	unsigned long size;
} PEDirEntry;

/* 128 bytes */
typedef struct {
	PEDirEntry pe_export_table;
	PEDirEntry pe_import_table;
	PEDirEntry pe_resource_table;
	PEDirEntry pe_exception_table;
	PEDirEntry pe_certificate_table;
	PEDirEntry pe_reloc_table;
	PEDirEntry pe_debug;
	PEDirEntry pe_copyright;
	PEDirEntry pe_global_ptr;
	PEDirEntry pe_tls_table;
	PEDirEntry pe_load_config_table;
	PEDirEntry pe_bound_import;
	PEDirEntry pe_iat;
	PEDirEntry pe_delay_import_desc;
	PEDirEntry pe_cli_header;
	PEDirEntry pe_reserved;
} PEDatadir;

/* 248 bytes */
typedef struct {
	char            pesig [4];
	COFFHeader  coff;
	PEHeader    pe;
	PEHeaderNT  nt;
	PEDatadir   datadir;
} DotNetHeader;

typedef struct {
	char    st_name [8];
	unsigned long st_virtual_size;
	unsigned long st_virtual_address;
	unsigned long st_raw_data_size;
	unsigned long st_raw_data_ptr;
	unsigned long st_reloc_ptr;
	unsigned long st_lineno_ptr;
	unsigned short st_reloc_count;
	unsigned short st_line_count;

#define SECT_FLAGS_HAS_CODE               0x20
#define SECT_FLAGS_HAS_INITIALIZED_DATA   0x40
#define SECT_FLAGS_HAS_UNINITIALIZED_DATA 0x80
#define SECT_FLAGS_MEM_DISCARDABLE        0x02000000
#define SECT_FLAGS_MEM_NOT_CACHED         0x04000000
#define SECT_FLAGS_MEM_NOT_PAGED          0x08000000
#define SECT_FLAGS_MEM_SHARED             0x10000000
#define SECT_FLAGS_MEM_EXECUTE            0x20000000
#define SECT_FLAGS_MEM_READ               0x40000000
#define SECT_FLAGS_MEM_WRITE              0x80000000
	unsigned long st_flags;

} SectionTable;

typedef struct {
	unsigned long        ch_size;
	unsigned short        ch_runtime_major;
	unsigned short        ch_runtime_minor;
	PEDirEntry ch_metadata;

#define CLI_FLAGS_ILONLY         0x01
#define CLI_FLAGS_32BITREQUIRED  0x02
#define CLI_FLAGS_TRACKDEBUGDATA 0x00010000
	unsigned long        ch_flags;

	unsigned long        ch_entry_point;
	PEDirEntry ch_resources;
	PEDirEntry ch_strong_name;
	PEDirEntry ch_code_manager_table;
	PEDirEntry ch_vtable_fixups;
	PEDirEntry ch_export_address_table_jumps;

	/* The following are zero in the current docs */
	PEDirEntry ch_eeinfo_table;
	PEDirEntry ch_helper_table;
	PEDirEntry ch_dynamic_info;
	PEDirEntry ch_delay_load_info;
	PEDirEntry ch_module_image;
	PEDirEntry ch_external_fixups;
	PEDirEntry ch_ridmap;
	PEDirEntry ch_debug_map;
	PEDirEntry ch_ip_map;
} CLIHeader;

/* This is not an on-disk structure */
typedef struct {
	DotNetHeader  cli_header;
	int               cli_section_count;
	SectionTable  *cli_section_tables;
	void            **cli_sections;
	CLIHeader     cli_cli_header;
} CLIImageInfo;

#endif /* __MONO_CIL_COFF_H__ */
